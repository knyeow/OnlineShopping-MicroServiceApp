FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ../../.env ./  
COPY ../../nuget.config ./
COPY ../../nupkgs ./nupkgs
COPY ../../src/UserService ./UserService


WORKDIR /app/UserService
RUN dotnet restore
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/UserService/out .

ENTRYPOINT ["dotnet", "UserService.dll"]
EXPOSE 80