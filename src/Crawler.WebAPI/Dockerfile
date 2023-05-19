FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Crawler.WebAPI/Crawler.WebAPI.csproj", "src/Crawler.WebAPI/"]
COPY ["src/Crawler.Shared/Crawler.Shared.csproj", "src/Crawler.Shared/"]
RUN dotnet restore "src/Crawler.WebAPI/Crawler.WebAPI.csproj"
COPY . .
WORKDIR "/src/src/Crawler.WebAPI"
RUN dotnet build "Crawler.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Crawler.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Crawler.WebAPI.dll"]
