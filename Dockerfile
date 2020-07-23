FROM microsoft/dotnet:2.2-sdk AS installer-env
COPY . /src/dotnet-function-app
RUN cd /src/dotnet-function-app && \
    mkdir -p /home/site/wwwroot && \
    dotnet publish *.csproj --output /home/site/wwwroot

# To enable ssh & remote debugging on app service change the base image to the one below
# FROM mcr.microsoft.com/azure-functions/dotnet:2.0-appservice 
FROM mcr.microsoft.com/azure-functions/dotnet:2.0
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

RUN apt-get update && apt-get install -y texlive-xetex wget unzip fonts-noto
RUN mkdir -p /opt/fontindexer
RUN wget https://github.com/WycliffeAssociates/FontIndexer/releases/download/1.0.1/linux-x64.zip -O /opt/fontindexer/linux-x64.zip
RUN unzip /opt/fontindexer/linux-x64.zip -d /opt/fontindexer/
RUN chmod u+x /opt/fontindexer/linux-x64/FontIndexer
RUN mkdir -p /tmp/wa/fonts
RUN cp /usr/share/fonts/truetype/noto/*-Regular.ttf /tmp/wa/fonts
RUN mkdir -p /home/site/wwwroot
RUN /opt/fontindexer/linux-x64/FontIndexer --source=/tmp/wa/fonts --output=/home/site/wwwroot/fonts.json
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]