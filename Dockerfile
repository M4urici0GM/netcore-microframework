FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

WORKDIR /src
COPY . .
RUN dotnet publish "tests-socket-net.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "tests-socket-net.dll"]
