FROM  mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /build/src

ENV NODE_VERSION 18.2.0
ENV NODE_DOWNLOAD_SHA 73d3f98e96e098587c2154dcaa82a6469a510e89a4881663dc4c86985acf245e
ENV NODE_DOWNLOAD_URL https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION-linux-x64.tar.gz

RUN wget "$NODE_DOWNLOAD_URL" -O nodejs.tar.gz \
	&& echo "$NODE_DOWNLOAD_SHA  nodejs.tar.gz" | sha256sum -c - \
	&& tar -xzf "nodejs.tar.gz" -C /usr/local --strip-components=1 \
	&& rm nodejs.tar.gz \
	&& ln -s /usr/local/bin/node /usr/local/bin/nodejs \
	&& curl -sL https://deb.nodesource.com/setup_16.x |  bash - \
	&& apt update \
	&& apt-get install -y nodejs

# --------------------------------------------------------------------------------------
# RESTORE 
# --------------------------------------------------------------------------------------
# - This section copies only the solution and projects files used to restore the needed 
#   NuGet packages.  The restored packages are placed in:  /root/.nuget/packages.
#
# - Since the source of the microservice will change the most often, these layers will
#   be reused and will not have be recreated when the updated source is built. 
# --------------------------------------------------------------------------------------

# Copy microservice components:
COPY ./src/Components/NetFusion.Identity.App/*.csproj ./Components/NetFusion.Identity.App/
COPY ./src/Components/NetFusion.Identity.Domain/*.csproj ./Components/NetFusion.Identity.Domain/
COPY ./src/Components/NetFusion.Identity.Infra/*.csproj ./Components/NetFusion.Identity.Infra/

# Copy microservice unit tests and web host:
COPY ./src/NetFusion.Identity.Tests/*.csproj ./NetFusion.Identity.Tests/
COPY ./src/NetFusion.Identity.Client/*.csproj ./NetFusion.Identity.Client/

# Copy the solution file to restore all projects:
COPY ./src/NetFusion.Identity.sln ./
RUN dotnet restore 

# # --------------------------------------------------------------------------------------
# # PUBLISH
# # --------------------------------------------------------------------------------------

# Copy all the source and build the microservice.
COPY ./src ./
RUN dotnet publish ./NetFusion.Identity.Client/NetFusion.Identity.Client.csproj -c release --output ../out  /p:DebugType=None

# --------------------------------------------------------------------------------------
# CREATE IMAGE FOR CONTAINER CREATION
# - This stage takes the published output and copies it to a layer
#   belonging to a new Docker image based on the runtime .net image. 
# --------------------------------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /microservice

COPY --from=build-env /build/out ./
ENTRYPOINT ["dotnet", "NetFusion.Identity.Client.dll"]
EXPOSE 80