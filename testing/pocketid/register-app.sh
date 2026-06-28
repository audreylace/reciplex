#!/bin/sh

# Configuration
POCKETID_URL="http://localhost:1411"
API_KEY="jqFHkKhJ0HAyNFuYeFTQoAmOxQHm0qer"

# create user
USER_RESPONSE=$(curl --silent --location "${POCKETID_URL}/api/users" \
  --header "X-API-KEY: ${API_KEY}" \
  --header "Content-Type: application/json" \
  -X POST \
  --data '{
    "username": "localuser",
    "email": "user@local.host",
    "isAdmin": true
  }')
echo "User API Response: $USER_RESPONSE"


# Call the internal OIDC clients endpoint
RESPONSE=$(curl --silent --location "${POCKETID_URL}/api/oidc/clients" \
  --header "X-API-KEY: ${API_KEY}" \
  --header "Content-Type: application/json" \
  -X POST \
  --data '{
    "id": "9bffd878-ddc5-4ebc-8ca4-b285f3f86a14",
    "name": "Reciplex",
    "redirectUris": [
      "http://localhost:1411/"
    ],
    "launchURL": "http://localhost:5173",
    "pkceEnabled": true,
    "client_name": "Reciplex",
    "grant_types": ["authorization_code"],
    "response_types": ["code"],
    "scope": "openid profile email"
  }')

echo $RESPONSE

# get client secret
SECRET=$(curl --silent --location "${POCKETID_URL}/api/oidc/clients/9bffd878-ddc5-4ebc-8ca4-b285f3f86a14/secret" \
  --header "X-API-KEY: ${API_KEY}" \
  -X POST)

echo $SECRET


