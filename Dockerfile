FROM node:24-alpine AS website
RUN corepack enable && corepack prepare yarn@4.x --activate
WORKDIR /build
COPY src/Reciplex.Client.Spa/package.json src/Reciplex.Client.Spa/yarn.lock src/Reciplex.Client.Spa/.yarnrc.yml ./
ENV CI=true
RUN yarn install --immutable
COPY src/Reciplex.Client.Spa ./
RUN yarn build

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS server

WORKDIR /build
COPY Directory.Packages.props reciplex.sln ./
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore reciplex.sln /p:Configuration=Release             

COPY src/ ./src
COPY --from=website /build/dist ./src/Reciplex.Server.Host/wwwroot
RUN dotnet publish src/Reciplex.Server.Host/Reciplex.Server.Host.csproj \
   --configuration Release \
   --no-restore \
   -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /opt/reciplex
COPY --from=server /build/out ./

EXPOSE 8080/tcp
EXPOSE 8443/tcp
USER $APP_UID
ENTRYPOINT ["dotnet", "/opt/reciplex/Reciplex.Server.Host.dll"]