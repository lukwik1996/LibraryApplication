FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY Library.Web/Library.Web.csproj Library.Web/
COPY CommonData/CommonData.csproj CommonData/
RUN dotnet restore Library.Web/Library.Web.csproj
COPY . .
WORKDIR /src/Library.Web
RUN dotnet build Library.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Library.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Library.Web.dll"]
