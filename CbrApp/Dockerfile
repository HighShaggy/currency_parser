FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CbrApp.csproj", "./"]
RUN dotnet restore "CbrApp.csproj"
COPY . .
RUN dotnet publish "CbrApp.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CbrApp.dll"]
