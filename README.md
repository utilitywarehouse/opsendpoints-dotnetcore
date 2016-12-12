# opsendpoints-dotnetcore
This is a
[middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
component, that adds endpoints to the request pipeline for the
[utilitywarehouse operational endpoints
specification](https://github.com/utilitywarehouse/operational-endpoints-spec).

## build

`dotnet restore` followed by `dotnet build` in the src/Utilitywarehouse.OpsEndpoints folder.

## test

`dotnet restore` followed by `dotnet test` in the test/Utilitywarehouse.OpsEndpoints.Tests folder.

## package

`dotnet pack -c Release` in the src/Utilitywarehouse.OpsEndpoints folder.

