FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source
COPY ./src ./src
COPY ./*.sln .
COPY ./*.props ./
COPY ./.editorconfig .

RUN dotnet restore "src/Itmo.Dev.Asap.Checker/Itmo.Dev.Asap.Checker.csproj"

FROM build AS publish
WORKDIR "/source/src/Itmo.Dev.Asap.Checker"
RUN dotnet publish "Itmo.Dev.Asap.Checker.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itmo.Dev.Asap.Checker.dll"]
