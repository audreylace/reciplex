FROM node:current-alpine3.22 AS website
WORKDIR /build
COPY website/package*.json ./
RUN npm install
COPY website ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server

WORKDIR /build
COPY src/Server/Database/Reciplex.Server.Database.csproj src/Server/Database/Reciplex.Server.Database.csproj
COPY src/Server/Host/Reciplex.Server.Host.csproj src/Server/Host/Reciplex.Server.Host.csproj
COPY src/Server/RecipeServices/Reciplex.Server.RecipeServices.csproj src/Server/RecipeServices/Reciplex.Server.RecipeServices.csproj
COPY src/Server/UserServices/Reciplex.Server.UserServices.csproj src/Server/UserServices/Reciplex.Server.UserServices.csproj
COPY Directory.Packages.props Directory.Packages.props
COPY server.sln server.sln
# Restore as distinct layers
RUN dotnet restore server.sln
COPY src/ ./src
COPY docker/website/appsettings.json src/Server/Host/appsettings.json
COPY --from=website /build/dist ./src/Server/Host/wwwroot
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=server /build/out .
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Reciplex.Server.Host.dll"]