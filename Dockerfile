FROM docker.io/library/node:24-bookworm-slim AS web
WORKDIR /src/sloppr-web
COPY sloppr-web/package.json sloppr-web/package-lock.json ./
RUN npm ci
COPY sloppr-web/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api
WORKDIR /src
COPY sloppr/sloppr.csproj sloppr/
RUN dotnet restore sloppr/sloppr.csproj
COPY sloppr/ sloppr/
RUN dotnet publish sloppr/sloppr.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=api --chown=app:app /app/publish/ ./
COPY --from=web --chown=app:app /src/sloppr-web/dist/sloppr-web/browser/ ./wwwroot/

ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://0.0.0.0:8080 \
    ConnectionStrings__DefaultConnection="Data Source=/data/sloppr.db"

RUN mkdir -p /data && chown app:app /data
VOLUME ["/data"]
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "sloppr.dll"]
