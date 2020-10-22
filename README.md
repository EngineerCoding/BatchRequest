<a href="https://www.nuget.org/packages/Ameling.BatchRequest#">
    <img src="https://img.shields.io/nuget/v/Ameling.BatchRequest?style=for-the-badge" alt="Get on NuGet!"/>
</a>

# BatchRequest

BatchRequest is NuGet package which opens up an endpoint within a Asp.Net Core application to make batch requests: multiple requests embedded within a single HTTP request. While REST API's are quite strong, sometimes lots of values have to be retrieved before proper UI can be shown. Launching multiple requests is definitely an option, but when the amount increases it is a good idea to bundle your requests to both make your UI code simpler and save some load on your server.

## Configuration

To add batch requests to your application, call the `IServiceCollection` extensions methond in `ConfigureServices(IServiceCollection services)`:

```
public void ConfigureServices(IServicesCollection services) 
{
    // ..
    services.AddBatchRequest();
    // ..
}
```

This method optionally takes an `Action<BatchRequestOptions>` parameter, to configure additional options:

* EnableEndpoint: Whether to enable the /api/batch endpoint. Defaults to true. When set to false, one can still get the `IBatchRequestService` through dependency injection.
* RequestHost: The request host which is set in `HttpContext.Request`, which could be used identify whether the current request is being executed in the context of a batch request. Defaults to `https://batchrequest`.
* DefaultProtocol: The request protocol string which is set in `HttpContext.Request`. Defaults to `BatchRequest`.

## Making a batch request

Each request is identified by the following model:
```
{
    "RelativeUri": "/api/batch", // The endpoint which has to be called on this server
    "Method": "Get", // The method to execute the endpoint with. Can be one of Delete, Get, Head, Options, Patch, Post or Put. When omitted (or null), defaults to Get
    "ContentType": "application/json", // The (optional) content type of the body which is posted
    "Body": "{\"my\": \"json object\"}", // The body to post
    "Base64Encoded": false // Whether the body is encoded with base64, for binary files. Defaults to false when omitted.
}
```

These objects are then put in an array, and if the endpoint is enabled can be posted to `/api/batch`. The response of this endpoint will be an array of the response objects, where each index corresponds with the same request. A response object is defined as:
```
{
    "StatusCode": 200, // the status code of the request
    "ContentType": "application/json; charset=utf-8;", // the response content type
    "Body": "5", // the response
    "Base64Encoded": false, // Whether the body was encoded with base64
}
```
