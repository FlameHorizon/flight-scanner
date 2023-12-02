FROM mcr.microsoft.com/dotnet/sdk:7.0 as build
WORKDIR /source
COPY . .

RUN dotnet restore "./FlightScannerApp/FlightScannerApp.csproj"
RUN dotnet publish "./FlightScannerApp/FlightScannerApp.csproj" -c release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:7.0.14-alpine3.18-arm64v8
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "FlightScannerApp.dll"]
