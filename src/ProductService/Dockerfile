FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ../../.env ./  
COPY ../../nuget.config ./
COPY ../../nupkgs ./nupkgs
COPY ../../src/ProductService ./ProductService


WORKDIR /app/ProductService
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/ProductService/out .



ENTRYPOINT ["dotnet", "ProductService.dll"]
EXPOSE 80