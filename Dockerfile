FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY ["src/CitiesApi/CitiesApi.csproj", "src/CitiesApi/"]

RUN dotnet restore "src/CitiesApi/CitiesApi.csproj"

COPY . .

RUN dotnet build "CitiesApi.sln" \
    --configuration Release \
    --no-restore

RUN dotnet publish "src/CitiesApi/CitiesApi.csproj" \
    --configuration Release \
    --no-build \
    --no-restore \
    --output /app

FROM base AS final
WORKDIR /app
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "CitiesApi.dll"]