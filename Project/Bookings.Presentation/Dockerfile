FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY . .

RUN dotnet restore Bookings.Presentation/Bookings.Presentation.csproj

RUN dotnet publish Bookings.Presentation/Bookings.Presentation.csproj \
    -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Bookings.Presentation.dll"]