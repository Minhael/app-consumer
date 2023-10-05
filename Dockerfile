#   https://somakdas.medium.com/private-nuget-package-restore-in-docker-build-azure-devops-431fdfeae557
#   https://stackoverflow.com/questions/63158785/azure-devops-nuget-artifact-feed-and-docker
#   https://docs.microsoft.com/en-us/azure/devops/pipelines/build/variables?view=azure-devops&tabs=yaml
#   https://docs.docker.com/develop/develop-images/build_enhancements/#new-docker-build-secret-information
#   https://docs.microsoft.com/en-us/dotnet/core/deploying/
#
#   export DOCKER_BUILDKIT=1
#   echo $(System.AccessToken) >> pat
#   docker build --build-arg BUILD_TYPE=Debug --secret id=pat,src=pat --progress=plain -t viziv-api -f App.WebApi/Dockerfile .
#   rm pat
#   docker run -v `pwd`/App.WebApi/src/Resources:/app/Resources -p 5000:5000 -e DbConnectionString="Server=sqlsrv,1433; Database=Master; User Id=SA; Password=P@ssw0rd;" -e EventHubConnectionString="Endpoint=sb://cds-web-vms-test-eh-0.servicebus.windows.net/;SharedAccessKeyName=HorizonAPI;SharedAccessKey=xxxx" -d viziv-api

#   Alpine missing gssntlm so use ubuntu instead
#   https://github.com/mikeTWC1984/gssntlm
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends gss-ntlmssp

WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

ARG BUILD_TYPE=Release
COPY . .
RUN dotnet restore "App.Consumer.csproj" -r linux-musl-x64 -p:UseAppHost=false -p:PublishReadyToRun=true

FROM build AS publish
RUN dotnet publish "App.Consumer.csproj" -c ${BUILD_TYPE}  -r linux-musl-x64 -o /app/publish --no-restore --self-contained false -p:UseAppHost=false -p:PublishReadyToRun=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App.Consumer.dll"]
