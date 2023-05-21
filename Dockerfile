FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY src .

RUN dotnet restore && dotnet build -c Release --no-restore
CMD chmod +x Banking.Simulation.Api
RUN dotnet publish "./Banking.Simulation.Api/Banking.Simulation.Api.csproj" -c Release -o /app/publish

ENV PATH="${PATH}:~/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef

RUN dotnet dev-certs https

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /root/.dotnet/corefx/cryptography/x509stores/my/* /root/.dotnet/corefx/cryptography/x509stores/my/

#ENTRYPOINT ["dotnet", "/app/Banking.Simulation.Api/Banking.Simulation.Api.dll"]
ENTRYPOINT ["/app/Banking.Simulation.Api"]