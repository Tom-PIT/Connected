FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /src
RUN mkdir /plugins
RUN mkdir /connected.logs
RUN apt-get update && apt-get install curl -y && apt-get install procps -y && apt-get install -y apt-utils libgdiplus libc6-dev && apt-get install -y jq 

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY . .

FROM build AS restore-build
WORKDIR /src
RUN mkdir /plugins
RUN dotnet restore "./Build/Build.sln"
RUN dotnet restore "./Sys/Sys.sln"
RUN dotnet restore "./App/App.sln"
RUN dotnet restore "./Management/Management.sln"
RUN dotnet restore "./Cdn/Cdn.sln"
RUN dotnet build "./Build/DevExpress/DevExpress.csproj" -c Release -o /build/devexpress
RUN dotnet publish "./Build/DevExpress/DevExpress.csproj" -c Release -o /publish/devexpress
RUN dotnet build "./Build/Microsoft/Microsoft.csproj" -c Release -o /build/microsoft
RUN dotnet publish "./Build/Microsoft/Microsoft.csproj" -c Release -o /publish/microsoft
RUN rm -rf plugins/
RUN mkdir ./plugins
RUN mkdir ./plugins/TomPIT.DataProviders.BigData
RUN mkdir ./plugins/TomPIT.DataProviders.Modbus
RUN mkdir ./plugins/TomPIT.DataProviders.Sql
RUN mkdir ./plugins/TomPIT.MicroServices
RUN mkdir ./plugins/TomPIT.MicroServices.BigData
RUN mkdir ./plugins/TomPIT.MicroServices.BigData.Design
RUN mkdir ./plugins/TomPIT.MicroServices.Design
RUN mkdir ./plugins/TomPIT.MicroServices.IoT
RUN mkdir ./plugins/TomPIT.MicroServices.IoT.Design
RUN mkdir ./plugins/TomPIT.MicroServices.Reporting
RUN mkdir ./plugins/TomPIT.MicroServices.Reporting.Design
RUN mkdir ./plugins/TomPIT.StorageProviders.Azure
RUN mkdir ./plugins/TomPIT.StorageProviders.Sql
RUN mkdir ./plugins/TomPIT.SysDb.Sql
RUN cp -r /publish/devexpress/. ./plugins/TomPIT.MicroServices.Reporting
RUN cp -r /publish/microsoft/. ./plugins/TomPIT.DataProviders.Sql
RUN dotnet build "./Build/Build.sln" -c Release -o /build/plugins
RUN dotnet build "./Sys/Sys.sln" -c Release -o /build/sys  
RUN dotnet build "./App/App.sln" -c Release -o /build/app
RUN dotnet build "./BigData/BigData.sln" -c Release -o /build/bigdata
RUN dotnet build "./Management/Management.sln" -c Release -o /build/mng
RUN dotnet build "./Cdn/Cdn.sln" -c Release -o /build/cdn
RUN dotnet build "./Search/Search.sln" -c Release -o /build/search
RUN dotnet build "./IoT/IoT.sln" -c Release -o /build/iot
RUN dotnet build "./Development/Development.sln" -c Release -o /build/dev
RUN dotnet build "./Worker/Worker.sln" -c Release -o /build/worker
RUN dotnet build "./Rest/Rest.sln" -c Release -o /build/rest
FROM restore-build AS publish-sys
WORKDIR /src
RUN dotnet publish "./Sys/Sys.sln" -c Release -o /app/sys /p:UseAppHost=false
FROM publish-sys AS publish-app
WORKDIR /src
RUN dotnet publish "./App/App.sln" -c Release -o /app/app /p:UseAppHost=false
FROM publish-app AS publish-mng
WORKDIR /src
RUN dotnet publish "./Management/Management.sln" -c Release -o /app/mng /p:UseAppHost=false
FROM publish-mng AS publish-cdn
WORKDIR /src
RUN dotnet publish "./Cdn/Cdn.sln" -c Release -o /app/cdn /p:UseAppHost=false
FROM publish-cdn AS publish-search
WORKDIR /src
RUN dotnet publish "./Search/Search.sln" -c Release -o /app/search /p:UseAppHost=false
FROM publish-search AS publish-iot
WORKDIR /src
RUN dotnet publish "./IoT/IoT.sln" -c Release -o /app/iot /p:UseAppHost=false
FROM publish-iot AS publish-dev
WORKDIR /src
RUN dotnet publish "./Development/Development.sln" -c Release -o /app/dev /p:UseAppHost=false
FROM publish-dev AS publish-worker
WORKDIR /src
RUN dotnet publish "./Worker/Worker.sln" -c Release -o /app/worker /p:UseAppHost=false
FROM publish-worker AS publish-rest
WORKDIR /src
RUN dotnet publish "./Rest/Rest.sln" -c Release -o /app/rest /p:UseAppHost=false
FROM publish-rest AS publish-bigdata
WORKDIR /src
RUN dotnet publish "./BigData/BigData.sln" -c Release -o /app/bigdata /p:UseAppHost=false

FROM base as final
# set sys/sys.json
ENV sys_config_db_connection="data source=localhost;initial catalog=templatesys;user id=sa;password=yourstrong(!)password;"
ENV sys_config_valid_issuer="tompit.net"
ENV sys_config_valid_audience="tompit.net"
ENV sys_config_issuer_signing_key="f221ececdb704efb9568a8756115ebec"
ENV sys_storage_provider="tompit.storageprovider.sql.sqlstorageprovider, tompit.storageprovider.sql"
ENV sys_database="TomPIT.SysDb.Sql.SqlProxy, TomPIT.SysDb.Sql"
# set rest of **/sys.json
ENV config_connection_name="localhost"
ENV config_connection_url="http://localhost:44001"
ENV config_connection_auth="defaultauthenticationtoken"
ENV config_plugins_path="/plugins"
ENV config_plugins_shadow_copy=true
# database sync
ENV db_sync=false
# install apps
ENV start_app=false
ENV start_mng=false
ENV start_cdn=false
ENV start_worker=false
ENV start_search=false
ENV start_dev=false
ENV start_rest=false
ENV start_iot=false
ENV start_bigdata=false

WORKDIR /app
COPY --from=publish-sys /app/sys ./sys
COPY --from=publish-app /app/app ./app
COPY --from=publish-mng /app/mng ./mng
COPY --from=publish-cdn /app/cdn ./cdn
COPY --from=publish-search /app/search ./search
COPY --from=publish-iot /app/iot ./iot
COPY --from=publish-dev /app/dev ./dev
COPY --from=publish-worker /app/worker ./worker
COPY --from=publish-rest /app/rest ./rest
COPY --from=publish-bigdata /app/bigdata ./bigdata
COPY --from=restore-build /src/plugins/ /plugins
COPY --from=restore-build /src/Build/Deploy/Plugins/ /plugins
COPY --from=build /src/setup/start-apps.sh .
COPY --from=build /src/setup/start-configuration.sh .
COPY --from=installer /src/installer /installer
RUN chmod +x ./start-apps.sh
RUN chmod +x ./start-configuration.sh

CMD ["./start-configuration.sh"]