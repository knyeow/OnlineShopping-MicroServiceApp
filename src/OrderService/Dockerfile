FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ../../.env ./  
COPY ../../nuget.config ./
COPY ../../nupkgs ./nupkgs
COPY ../../src/OrderService ./OrderService


WORKDIR /app/OrderService
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/OrderService/out .

ENTRYPOINT ["dotnet", "OrderService.dll"]
EXPOSE 80