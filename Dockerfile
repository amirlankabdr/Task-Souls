FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/QuestForge.Core/QuestForge.Core.csproj src/QuestForge.Core/
COPY src/QuestForge.Infrastructure/QuestForge.Infrastructure.csproj src/QuestForge.Infrastructure/
COPY src/QuestForge.Web/QuestForge.Web.csproj src/QuestForge.Web/
RUN dotnet restore src/QuestForge.Web/QuestForge.Web.csproj
COPY . .
RUN dotnet publish src/QuestForge.Web/QuestForge.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0.13
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "QuestForge.Web.dll"]
