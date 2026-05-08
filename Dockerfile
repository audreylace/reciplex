FROM node:24 AS website
RUN corepack enable && corepack prepare yarn@4.x --activate
WORKDIR /build
COPY src/Reciplex.Client.Spa/package*.json ./
COPY src/Reciplex.Client.Spa/yarn.lock ./yarn.lock
COPY src/Reciplex.Client.Spa/.yarnrc.yml ./.yarnrc.yml
RUN yarn install
COPY src/Reciplex.Client.Spa ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS server

WORKDIR /build
COPY src/*/*.csproj ./
COPY Directory.Packages.props Directory.Packages.props
COPY reciplex.sln .
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore reciplex.sln
COPY src/ ./src
COPY docker/appsettings.json src/Reciplex.Server.Host/appsettings.json
COPY --from=website /build/dist ./src/Reciplex.Server.Host/wwwroot
RUN ls ./src/Reciplex.Server.Host/wwwroot
RUN dotnet publish -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=server /build/out .
EXPOSE 8080/tcp
ENTRYPOINT ["dotnet", "Reciplex.Server.Host.dll"]