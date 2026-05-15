# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# Copiar csproj e restaurar dependências
COPY *.csproj .
RUN dotnet restore

# Copiar o restante e publicar
COPY . .
RUN dotnet publish -c Release -o /app

# Estágio Final
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app .

# Render utiliza a variável de ambiente PORT. 
# Configuramos o ASP.NET para ouvir nessa porta.
ENV ASPNETCORE_URLS=http://+:10000

# Expor a porta que o Render costuma usar (padrão)
EXPOSE 10000

ENTRYPOINT ["dotnet", "VotoTrack.dll"]
