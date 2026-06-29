# Reciplex

Reciplex is a self-hosted web application for organizing and viewing recipes formatted with Markdown.
It features a modern frontend built with React and Material UI (`@mui`), backed by a lightweight,
ASP.NET core service utilizing SQLite3.

> ⚠️ **Status:** Reciplex is currently a work in progress (Alpha quality).

## Prerequisites & Installation

Reciplex is designed to run in lightweight, rootless container environments and has been fully tested using
[Podman](https://podman.io).

Because Reciplex does not currently distribute pre-built container images, you will need to build
the image locally from the repository root.

### Build the Container Image

To build the image and tag it automatically using the short Git hash of your current branch checkout, run:

```bash
podman build -t reciplex-server:$(git rev-parse --short HEAD) .
```

For easier local deployment, tag this freshly built image as `latest`:

```bash
podman tag localhost/reciplex-server:$(git rev-parse --short HEAD) localhost/reciplex-server:latest
```

### Configuration

Reciplex relies on an external OpenID Connect (OIDC) identity server (such as Authelia, Keycloak, or PocketID)
to handle user authentication.

Create a configuration file on your host machine at `~/.config/reciplex/appsettings.json`
and paste the following boilerplate:

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
      "Authority": "https://your-oidc-server.example.com"
    },
    "OidcSecrets": {
      "ClientId": "your-oidc-client-id",
      "ClientSecret": "your-oidc-client-secret"
    },
    "Routing": {
      "Domain": "https://your-reciplex-domain.example.com"
    },
    "Sqlite": {
      "Enable": true,
      "DatabaseConnection": "Data Source=/var/opt/reciplex/recipe-book.db;Mode=ReadWriteCreate;Cache=Private;",
      "EnableMigrations": true
    }
  }
}
```

### Running the Application

Once your configuration is saved and your image is built,
spin up the container using the following single command.
This handles data persistence for your recipes and binds the
required configuration file safely into the container.

```bash
podman run -d \
  --name reciplex \
  -p 5000:5000 \
  -v ~/.config/reciplex/appsettings.json:/etc/opt/reciplex/appsettings.json:ro \
  -v reciplex-data:/var/opt/reciplex:Z \
  localhost/reciplex-server:latest \
  -R -C /etc/opt/reciplex/appsettings.json
```

- `-d`: Runs the container detached in the background.
- `-p 5000:5000`: Maps port 5000 of your host to port 5000 inside the container. Access the web UI at http://localhost:5000.
- `-v .../appsettings.json:...:ro`: Mounts your host configuration file into the container as Read-Only (`:ro`) for security
- `-v reciplex-data:/var/opt/reciplex:Z`: Creates a persistent named volume to save your SQLite database. The `:Z` flag ensures correct SELinux permissions if running on distributions like Fedora, RHEL, or Rocky Linux.
- `-R -C ...`: Commands passed directly to the ASP.NET engine telling it to reset the default lookup pathways and prioritize your mounted configuration file.

## Support and License

Reciplex is licensed under the permissive Apache 2.0 License. It
comes with absolutely no warranty or official support, and is provided strictly "as is."

Users of this application assume all responsibility, maintenance, and labor that running it entails.

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for how to contribute to this project.
