FROM node:24-alpine AS website
RUN corepack enable && corepack prepare yarn@4.x --activate
WORKDIR /build
COPY --link src/Reciplex.Client.Spa/package.json src/Reciplex.Client.Spa/yarn.lock src/Reciplex.Client.Spa/.yarnrc.yml ./
ENV CI=true
RUN yarn install --immutable
COPY --link src/Reciplex.Client.Spa ./
RUN yarn build

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS server

WORKDIR /build
COPY --link Directory.Packages.props reciplex.sln ./
COPY --parents --link src/*/*.csproj ./
RUN dotnet restore reciplex.sln /p:Configuration=Release             

COPY --link src/ ./src
COPY --link --from=website /build/dist ./src/Reciplex.Server.Host/wwwroot
RUN dotnet publish src/Reciplex.Server.Host/Reciplex.Server.Host.csproj \
   --configuration Release \
   --no-restore \
   -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
WORKDIR /opt/reciplex
COPY --link --from=server /build/out ./

EXPOSE 8080/tcp
EXPOSE 8443/tcp
USER $APP_UID
ENTRYPOINT ["dotnet", "/opt/reciplex/Reciplex.Server.Host.dll"]