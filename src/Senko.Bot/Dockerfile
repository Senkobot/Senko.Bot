FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /proj
COPY . .
RUN ls /proj
RUN dotnet restore
WORKDIR "/proj/src/Senko.Bot"
ARG Build_SourceBranch
ENV Build_SourceBranch=$Build_SourceBranch
ARG Build_BuildID
ENV Build_BuildID=$Build_BuildID
RUN dotnet build "Senko.Bot.csproj" -c Release -o /app --no-restore

FROM build AS publish
RUN dotnet publish "Senko.Bot.csproj" -c Release -o /app

FROM base AS final
ADD https://github.com/ufoscout/docker-compose-wait/releases/download/2.5.1/wait /wait
RUN chmod +x /wait
WORKDIR /app
COPY --from=publish /app .
CMD /wait && dotnet Senko.Bot.dll