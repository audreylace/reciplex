FROM node:24 AS website
RUN corepack enable && corepack prepare yarn@4.x --activate
WORKDIR /build
COPY --link src/Reciplex.Client.Spa/package*.json ./
COPY --link src/Reciplex.Client.Spa/yarn.lock ./yarn.lock
COPY --link src/Reciplex.Client.Spa/.yarnrc.yml ./.yarnrc.yml
ENV CI=true
RUN yarn install --immutable
COPY --link src/Reciplex.Client.Spa ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS server

WORKDIR /build
COPY --link src/*/*.csproj ./
COPY --link Directory.Packages.props Directory.Packages.props
COPY --link reciplex.sln .
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore reciplex.sln
COPY --link src/ ./src
COPY --link --from=website /build/dist ./src/Reciplex.Server.Host/wwwroot
RUN dotnet publish --configuration Release --no-restore -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled
COPY --link --from=server /build/out /opt/reciplex
WORKDIR /opt/reciplex
EXPOSE 8080/tcp
EXPOSE 8443/tcp
USER $APP_UID
ENTRYPOINT ["dotnet", "/opt/reciplex/Reciplex.Server.Host.dll"]