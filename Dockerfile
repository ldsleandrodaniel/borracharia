# Estágio de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copia arquivos necessários
COPY ["*.csproj", "."]
RUN dotnet restore "borracharia.csproj"

# 2. Copia o resto
COPY . .

# 3. Publica o projeto (agora na raiz /publish)
RUN dotnet publish "borracharia.csproj" -c Release -o /publish \
    -p:RuntimeIdentifier=linux-x64 \
    --self-contained false

# Estágio de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# 4. Instala dependências para PostgreSQL
RUN apt-get update && \
    apt-get install -y libgdiplus libpq-dev && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 5. Configurações essenciais
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# 6. Copia os arquivos publicados (agora da raiz /publish)
COPY --from=build /publish .

# 7. Entrypoint
ENTRYPOINT ["dotnet", "borracharia.dll"]