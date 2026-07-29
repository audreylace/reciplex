# Setup on Debian using Podman, Caddy, and PocketId

This guide is for setting up Reciplex on a Debian system. The container
engine that will be used is Podman. PocketId will serve as the OpenID Connect
server. Caddy will provide HTTPS certificate management and termination.

## Setup the base machine

### 1. Install Podman & Prerequisites

Install Podman along with uidmap to facilitate rootless user and group mappings.

```bash
sudo apt-get update
sudo apt-get -y install podman uidmap
```

### 2. Base Host Configurations

Execute these steps to tune the host kernel, systemd network dependencies, and shared storage layers.
Increase Per-User Key Ring Limits: Prevents exhaustion from container lifecycles.

```bash
sudo tee /etc/sysctl.d/99-keys.conf << 'EOF'
kernel.keys.maxkeys = 20000
kernel.keys.maxbytes = 500000
EOF

sudo sysctl -p /etc/sysctl.d/99-keys.conf
```

**Enable Network-Online Target**: Prevents a default 90-second systemd startup delay on Debian.

```bash
sudo systemctl enable systemd-networkd-wait-online.service
```

**Initialize Central Image Repository**: Provides a shared workspace for offline container tarballs.

```bash
sudo mkdir -p /var/lib/shared-container-tarballs
sudo chmod -R 755 /var/lib/shared-container-tarballs
```

**Enable packet forwarding**: nftables will route 80/443 to the podman caddy container

```bash
sudo sysctl -w net.ipv4.ip_forward=1
# To make it permanent:
echo "net.ipv4.ip_forward=1" | sudo tee /etc/sysctl.d/99-forwarding.conf
```

### 3. Verify Basic Host Installation

Run standard images to confirm rootless containerization and fundamental networking function.

```bash
# Verify basic runtime execution
podman run --name hello-world docker.io/hello-world

# Verify rootless port mapping with Nginx; Open the server in your browser
podman run --name basic_httpd -d -p 8080:80/tcp docker.io/nginx

# Cleanup validation containers
podman stop hello-world basic_httpd
podman rm hello-world basic_httpd
podman ps -a
```

### Common Problems

Debian minimal cloud images fails to have the needed packages for a podman ssh session to function properly.

**Symptom:**

```
WARN[0000] The cgroupv2 manager is set to systemd but there is no systemd user session available
WARN[0000] For using systemd, you may need to log in using a user session
WARN[0000] Alternatively, you can enable lingering with: `loginctl enable-linger 1000` (possibly as root)
WARN[0000] Falling back to --cgroup-manager=cgroupfs
```

**Resolution:**
Install needed packages for the ssh session to properly start a systemd session and then reboot the server.

```
sudo apt update && sudo apt install -y libpam-systemd dbus-user-session
sudo loginctl enable-linger $(id -u)
sudo reboot
```

## Infrastructure setup

In production, Reciplex and its support services run inside a unprivileged user account to limit system level
damage in the event of a misbehaving app or security exploitation.

### 1. User & Environment Setup

Isolate the application ecosystem inside a dedicated system user account.

```bash
# Create application service user
sudo useradd -u 2001 -m reciplex
sudo usermod -s /usr/sbin/nologin reciplex

# Configure Sub-UID/GID allocation blocks
sudo usermod --add-subuids 300000-365535 --add-subgids 300000-365535 reciplex

# Enable systemd lingering for boot persistence
sudo loginctl enable-linger reciplex
```

**(Optional) Systemd Resource Allocation Slice**
Note: Execute this step only if deploying on a shared server. Skip for dedicated application VMs.

```bash
sudo mkdir -p /etc/systemd/system/user-2001.slice.d

sudo tee /etc/systemd/system/user-2001.slice.d/99-limits.conf << 'EOF'
[Slice]
CPUQuota=200%
MemoryHigh=1700M
MemoryMax=2048M
MemorySwapMax=2048M
EOF

sudo systemctl daemon-reload
```

### 2. Secret & Configuration Staging

Log in or assume the identity of the target deployment user to stage application parameters and sensitive cryptographic materials.

```bash
# Switch to the reciplex context (or perform tasks inside its home scope)
mkdir -p ~/.secret-staging ~/.config/containers/systemd ~/.config/caddy ~/.config/reciplex
chmod 700 ~/.secret-staging
```

**Staging Secret Materials**

To get `ClientId` and `ClientSecret` likely involves setting up Caddy and PocketId first. Once those are setup and confirmed running, add the Reciplex application to PocketId and then populate these values.

```
# 1. PocketID Master Key
openssl rand -base64 32 >> ~/.secret-staging/pocketid.key

# 2. OpenID Connect Secrets Configurations
cat << 'EOF' > ~/.secret-staging/oidc-secrets.settings.json
{
  "Reciplex": {
    "OidcSecrets": {
      "ClientId": "<< open id connect client Id >>",
      "ClientSecret": "<< open id connect client secret >>"
    }
  }
}
EOF

# 3. Data Protection Keypairs
openssl genrsa -out ~/.secret-staging/dp_key.pem 4096
openssl req -new -x509 -key ~/.secret-staging/dp_key.pem -out ~/.secret-staging/dp_cert.pem -days 365 -subj "/CN=ReciplexDataProtectionCert"

# 4. Import files directly into Podman Secret Store
podman secret create pocketidKey ~/.secret-staging/pocketid.key
podman secret create reciplexOidcSecretsSettingsJson ~/.secret-staging/oidc-secrets.settings.json
podman secret create reciplexDpKey ~/.secret-staging/dp_key.pem
podman secret create reciplexDpCert ~/.secret-staging/dp_cert.pem

# 5. Securely purge staging workspace
rm -rf ~/.secret-staging
```

**Reciplex Application Settings (`~/.config/reciplex/appsettings.json`)**

```json
{
  "Urls": "http://0.0.0.0:5000",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Reciplex": {
    "Oidc": {
      "Enable": true,
      "Authority": "https://your-pocketid-server.example.com",
      "InsecureDisableHttps": true,
      "BackChannelHostOverride": "http://pocketid:1411"
    },
    "Routing": {
      "Domain": "https://reciplex-application-domain.example.com",
      "InsecureTrustProxy": true
    },
    "ShortIds": {
      "MinLength": 16,
      "Alphabet": "<< short id alphabet >>"
    },
    "DataProtection": {
      "KeyStorageDirectory": "/var/opt/reciplex/data-protection",
      "EncryptionCertificate": "/etc/opt/reciplex/data-protection/cert.pem",
      "EncryptionPrivateKey": "/etc/opt/reciplex/data-protection/key.pem"
    },
    "Sqlite": {
      "Enable": true,
      "DatabaseConnection": "Data Source=/var/opt/reciplex/recipe-book.db;Mode=ReadWriteCreate;Cache=Private;",
      "EnableMigrations": true
    }
  }
}
```

**Caddy Application Settings (`~/.config/caddy/Caddyfile`)**

```
{
    admin off
    http_port  8080
    https_port 8443
}

your-pocketid-server.example.com{
    reverse_proxy 127.0.0.1:1411
    header Alt-Svc "h3=\":443\"; ma=2592000"
}

reciplex-application-domain.example.com {
    reverse_proxy 127.0.0.1:5000
    header Alt-Svc "h3=\":443\"; ma=2592000"
}
```

### 3. Build Reciplex Image

We need to build Reciplex from source since it is not distributed publicly in any container registry. These steps
assume you are building the image on the host machine under a dedicated user with sudo privileges. The build
creates a tarball that is loaded into a central location. The dedicated user then pulls that tarball back
into their local podman instance and runs it.

```bash
mkdir -p ~/projects
git clone https://github.com/audreylace/reciplex.git
cd reciplex.git

# Compile production image target
COMMIT_HASH=$(git rev-parse --short HEAD)
podman build -t reciplex-server:${COMMIT_HASH} .

# Export compilation artifacts to global storage archive
podman save -o /tmp/reciplex-${COMMIT_HASH}.tar localhost/reciplex-server:${COMMIT_HASH}
sudo mv /tmp/reciplex-${COMMIT_HASH}.tar /var/lib/shared-container-tarballs
sudo chown root:root /var/lib/shared-container-tarballs/reciplex-${COMMIT_HASH}.tar
```

In the `reciplex` user pull the image over:

```bash
# Load production artifact into runtime instance space
podman load -i /var/lib/shared-container-tarballs/reciplex-${COMMIT_HASH}.tar
podman tag localhost/reciplex-server:${COMMIT_HASH} localhost/reciplex-server:latest
```

### 4. Quadlet Systemd Unit Manifests

Drop these standard Podman Quadlet configuration definitions into `~/.config/containers/systemd/`.
Systemd will dynamically manage container lifecycles from these files.

**Internal Shared Network (`reciplex.network`)**

```Ini, TOML
[Network]
NetworkName=reciplex
```

**Caddy Container (`caddy.container`)**

```Ini, TOML
[Container]
ContainerName=caddy
Image=docker.io/library/caddy:alpine
Network=host
UserNS=auto:size=2000
PublishPort=8080:8080
PublishPort=8443:8443
Volume=%h/.config/caddy:/etc/caddy:ro
Volume=caddy-volume:/data:U
AutoUpdate=registry

[Service]
Restart=always
CPUQuota=200%
MemoryHigh=768M
MemoryMax=1G
MemorySwapMax=1G

[Install]
WantedBy=default.target
```

**Caddy Volume (`caddy.volume`)**

```Ini, TOML
[Volume]
VolumeName=caddy-volume
Label=Stores Caddy data
```

**PocketID Container (`pocketid.container`)**

```Ini, TOML
[Container]
ContainerName=pocketid
Image=ghcr.io/pocket-id/pocket-id:latest
Network=reciplex.network
Volume=pocketid-volume:/app/data:U
UserNS=auto:size=2000
User=1000:1000
Secret=pocketidKey,target=/etc/opt/pocketid/encryption_key,mode=0440,uid=0,gid=1000
AutoUpdate=registry

PublishPort=127.0.0.1:1411:1411

HealthCmd=/app/pocket-id healthcheck
HealthInterval=1m30s
HealthTimeout=5s
HealthRetries=2
HealthStartPeriod=10s

Environment=APP_URL=https://your-pocketid-server.example.com
Environment=TRUST_PROXY=true
Environment=ENCRYPTION_KEY_FILE=/etc/opt/pocketid/encryption_key

[Service]
Restart=always
CPUQuota=100%
MemoryHigh=180M
MemoryMax=256M
MemorySwapMax=256M

[Install]
WantedBy=default.target
```

**PocketID Volume (`pocketid.volume`)**

```Ini, TOML
[Volume]
VolumeName=pocketid-volume
Label=Stores Pocketid data
```

**Reciplex Container (`reciplex.container`)**

```Ini, TOML
[Container]
ContainerName=reciplex
Image=reciplex-server:latest
Network=reciplex.network
Volume=reciplex-volume:/var/opt/reciplex:U
UserNS=auto:size=2000
User=1654:1654
AutoUpdate=local

Volume=%h/.config/reciplex/appsettings.json:/etc/opt/reciplex/appsettings.json:ro
Secret=reciplexOidcSecretsSettingsJson,target=/etc/opt/reciplex/oidc-secrets.settings.json,mode=0440,uid=0,gid=1654
Secret=reciplexDpKey,target=/etc/opt/reciplex/data-protection/key.pem,mode=0440,uid=0,gid=1654
Secret=reciplexDpCert,target=/etc/opt/reciplex/data-protection/cert.pem,mode=0440,uid=0,gid=1654

Exec=-R -C /etc/opt/reciplex/appsettings.json -C /etc/opt/reciplex/oidc-secrets.settings.json

PublishPort=127.0.0.1:5000:5000

[Service]
Restart=always
CPUQuota=200%
MemoryHigh=420M
MemoryMax=512M
MemorySwapMax=512M

[Install]
WantedBy=default.target
```

**Reciplex Volume (`reciplex.volume`)**

```Ini, TOML
[Volume]
VolumeName=reciplex-volume
Label=Stores Reciplex data
```

### 5. Start containers

```bash
# Only invoke if systemctl fails saying it can't find dbus socket
export XDG_RUNTIME_DIR=/run/user/$(id -u)

# pull in config
systemctl --user daemon-reload

# Start the cluster units
systemctl --user start caddy pocketid reciplex
```

### 5. Configure nftables

Use nftables to map low ports `80` and `443` to caddy ports at `8080` and `8443`. Put at `/etc/nftables.conf`.

```
#!/usr/sbin/nft -f

flush ruleset

table inet filter {
	chain input {
		type filter hook input priority filter;
	}
	chain forward {
		type filter hook forward priority filter;
	}
	chain output {
		type filter hook output priority filter;
	}
}

# Create the NAT table if it doesn't exist
table ip nat {
    # Forward external traffic hitting ports 80/443 to Caddy
    chain prerouting {
        type nat hook prerouting priority dstnat; policy accept;
        tcp dport 80 redirect to :8080
        tcp dport 443 redirect to :8443
        # Redirect HTTP/3 / QUIC (UDP 443 -> 8443)
        udp dport 443 redirect to :8443
    }

    # Forward local host traffic (localhost) hitting ports 80/443 to Caddy
    chain output {
        type nat hook output priority dstnat; policy accept;
        oifname "lo" tcp dport 80 redirect to :8080
        oifname "lo" tcp dport 443 redirect to :8443
    }
}
```

Enable the service at boot:

```
# Enable at boot
sudo systemctl enable nftables.service
# Apply now
sudo systemctl start nftables.service
```
