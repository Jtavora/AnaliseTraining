# Use a imagem base do SDK do .NET para build e execução
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime

# Definir diretório de trabalho
WORKDIR /app

# Copiar todos os arquivos da aplicação para o container
COPY . ./

# Restaurar dependências
RUN dotnet restore

# Compilar a aplicação
RUN dotnet build --no-restore -c Release

# Instalar a ferramenta dotnet-ef globalmente
RUN dotnet tool install --global dotnet-ef

# Adicionar o diretório de ferramentas globais ao PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copie o script wait-for-it
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh

CMD ["./wait-for-it.sh", "db:5432", "--", "bash", "-c", "dotnet ef migrations add NewmMigration && dotnet ef database update && dotnet run --no-build --urls http://0.0.0.0:5171"]