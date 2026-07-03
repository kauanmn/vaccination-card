# syntax=docker/dockerfile:1

# ---------- Stage 1: build do SPA (React + Vite) ----------
FROM node:22-slim AS frontend
WORKDIR /frontend

# Instala dependências primeiro (cache de layer)
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

# Copia o código e gera o build estático em /frontend/dist
COPY frontend/ ./
RUN npm run build

# ---------- Stage 2: build/publish da API (.NET) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /repo

# Restaura só com os .csproj para aproveitar cache de layer
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj src/Api/
RUN dotnet restore src/Api/Api.csproj

# Copia o restante do código e publica
COPY src/ ./src/
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Stage 3: runtime (serve API + SPA no mesmo container) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Binários da API
COPY --from=backend /app/publish ./
# SPA publicado vai para wwwroot (servido por UseStaticFiles / fallback)
COPY --from=frontend /frontend/dist ./wwwroot

# Diretório do banco SQLite (montável como volume para persistir dados)
RUN mkdir -p /app/data

ENV ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Data Source=/app/data/vaccinationcard.db"

EXPOSE 8080

# Usa $PORT quando a plataforma injeta (ex.: Render); senão 8080 (local/Fly)
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet Api.dll"]
