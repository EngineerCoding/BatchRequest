using BatchRequest.Abstractions;
using BatchRequest.Attributes;
using BatchRequest.Exceptions;
using BatchRequest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRequest
{
	internal class BatchRequestService : IBatchRequestService
	{
		private const char QuerySeparator = '?';
		private const char ContentTypeSeparator = ';';
		private const string CharsetMarker = "charset=";

		/// <summary>
		/// The http context accessor
		/// </summary>
		private readonly IHttpContextAccessor _httpContextAccessor;
		/// <summary>
		/// The service provider to set on the newly created HttpContext
		/// </summary>
		private readonly IServiceProvider _serviceProvider;
		/// <summary>
		/// All available route endpoints
		/// </summary>
		private readonly RouteEndpoint[] _routeEndpoints;
		/// <summary>
		/// The batch request options
		/// </summary>
		private readonly BatchRequestOptions _batchRequestOptions;

		/// <summary>
		/// A dictionary which serves as cache for retrieving the <see cref="TemplateMatcher"/> object
		/// </summary>
		private static readonly ConcurrentDictionary<string, TemplateMatcher> _templateMatcherCache = new ConcurrentDictionary<string, TemplateMatcher>();

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="serviceProvider">The injected service provider</param>
		/// <param name="endpointDataSource">The injected endpoint datasource</param>
		/// <param name="batchRequestOptions">The injected batch request options</param>
		public BatchRequestService(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider, EndpointDataSource endpointDataSource, BatchRequestOptions batchRequestOptions)
		{
			_httpContextAccessor = httpContextAccessor;
			_serviceProvider = serviceProvider;
			_routeEndpoints = endpointDataSource.Endpoints
				.Select(endpoint => endpoint as RouteEndpoint)
				.Where(endpoint => endpoint != null)
				.ToArray();
			_batchRequestOptions = batchRequestOptions;
		}

		/// <inheritdoc/>
		public bool Run(IEnumerable<RequestInfo> requestInfos, out IEnumerable<RequestResult> requestResults)
		{
			IEnumerable<Func<Task<HttpContext>>> httpContextFuncs = null;

			try
			{
				httpContextFuncs = TransformToInternalRequests(requestInfos);
			}
			catch (Base64FormatException)
			{
				// Fallback on the null check, which is required anyway
			}

			if (httpContextFuncs == null)
			{
				requestResults = null;
				return false;
			}

			Task<HttpContext>[] httpContextTasks = httpContextFuncs.Select(func => func.Invoke()).ToArray();
			Task.WaitAll(httpContextTasks);

			requestResults = TransformToRequestResults(httpContextTasks.Select(task => task.Result));
			return true;
		}

		/// <inheritdoc/>
		public IEnumerable<Func<Task<HttpContext>>> TransformToInternalRequests(IEnumerable<RequestInfo> requestInfos)
		{
			RequestInfo[] allRequestInfos = requestInfos.ToArray();
			// Check if all requestInfo's contain a valid HttpMethod
			string[] httpMethods = Enum.GetValues(typeof(HttpMethod))
				.Cast<HttpMethod>()
				.Select(method => method.ToString().ToLower())
				.ToArray();
			if (!allRequestInfos.Select(requestInfo => requestInfo.Method.ToLower()).All(httpMethods.Contains))
			{
				return null;
			}

			bool[] processedRequestInfos = new bool[allRequestInfos.Length];
			Func<Task<HttpContext>>[] resultFuncs = new Func<Task<HttpContext>>[allRequestInfos.Length];
			foreach (RouteEndpoint routeEndpoint in _routeEndpoints)
			{
				for (int i = 0; i < resultFuncs.Length; i++)
				{
					if (processedRequestInfos[i])
					{ // Already processed this RequestInfo object
						continue;
					}

					RequestInfo requestInfo = allRequestInfos[i];
					EndpointMatchResult matchResult = EndpointMatches(routeEndpoint, requestInfo, out RouteValueDictionary routeValues);
					if (matchResult == EndpointMatchResult.Match || matchResult == EndpointMatchResult.Ignored)
					{
						processedRequestInfos[i] = true;
						if (matchResult == EndpointMatchResult.Ignored)
						{
							resultFuncs[i] = GetNotFoundResult;
							continue;
						}
					}
					else if (matchResult == EndpointMatchResult.NoMatch)
					{
						continue;
					}
					else
					{
						throw new NotImplementedException("Match result " + matchResult.ToString() );
					}

					// Attempt to build the HTTP Context
					HttpContext httpContext = BuildHttpContext(routeEndpoint, requestInfo, routeValues);
					resultFuncs[i] = () =>
					{
						TaskCompletionSource<HttpContext> taskCompletionSource = new TaskCompletionSource<HttpContext>();
						routeEndpoint.RequestDelegate.Invoke(httpContext).ContinueWith(task =>
						{
							if (task.IsFaulted)
							{
								taskCompletionSource.SetException(task.Exception);
							}
							else if (task.IsCanceled)
							{
								taskCompletionSource.SetCanceled();
							}
							else
							{
								taskCompletionSource.SetResult(httpContext);
							}
						});

						return taskCompletionSource.Task;
					};
				}
			}

			// Not all supplied requestInfo's have to be found at this point
			// Set the results to 404 where the request info has not been found
			for (int i = 0; i < processedRequestInfos.Length; i++)
			{
				if (!processedRequestInfos[i])
				{
					resultFuncs[i] = GetNotFoundResult;
				}
			}

			return resultFuncs;
		}

		/// <inheritdoc/>
		public IEnumerable<RequestResult> TransformToRequestResults(IEnumerable<HttpContext> httpContexts)
		{
			foreach (HttpContext httpContext in httpContexts)
			{
				string body = null;
				bool isBase64 = false;
				if (httpContext.Response.Body != null && httpContext.Response.ContentType?.Contains(CharsetMarker) == true)
				{
					MemoryStream memoryStream = new MemoryStream();
					using (httpContext.Response.Body)
					{
						httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
						httpContext.Response.Body.CopyTo(memoryStream);
					}
					byte[] data = memoryStream.ToArray();
					if (data.Length > 0)
					{
						string charset = httpContext.Response.ContentType.Split(ContentTypeSeparator)
							.Select(component => component.Trim())
							.Where(component => component.StartsWith(CharsetMarker))
							.FirstOrDefault()?.Substring(CharsetMarker.Length);

						if (!string.IsNullOrEmpty(charset))
						{
							Encoding encoding = Encoding.GetEncoding(charset);
							body = encoding.GetString(data);
						}
						else
						{
							isBase64 = true;
							body = Convert.ToBase64String(memoryStream.GetBuffer());
						}
					}
				}

				yield return new RequestResult()
				{
					StatusCode = httpContext.Response.StatusCode,
					ContentType = httpContext.Response.ContentType,
					Body = body,
					Base64Encoded = isBase64,
				};
			}
		}

		/// <summary>
		/// Builds the HTTP context.
		/// </summary>
		/// <param name="routeEndpoint">The route endpoint.</param>
		/// <param name="requestInfo">The request information.</param>
		/// <param name="routeValues">The route values.</param>
		/// <returns>The HTTP context to invoke a request delegate with</returns>
		private HttpContext BuildHttpContext(RouteEndpoint routeEndpoint, RequestInfo requestInfo, RouteValueDictionary routeValues)
		{
			DefaultHttpContext httpContext = new DefaultHttpContext();
			httpContext.Features.Set<IEndpointFeature>(new HttpContextFeatures.EndpointFeature(routeEndpoint));
			httpContext.Features.Set<IRoutingFeature>(new HttpContextFeatures.RoutingFeature(routeValues));
			httpContext.RequestServices = _serviceProvider;

			if (_httpContextAccessor.HttpContext != null)
			{
				httpContext.User = _httpContextAccessor.HttpContext.User;
			}

			string[] relativeUriComponents = requestInfo.RelativeUri.Split(QuerySeparator);
			Uri requestUri = new UriBuilder(_batchRequestOptions.RequestHost)
			{
				Path = relativeUriComponents[0],
				Query = relativeUriComponents.Length > 1 ? relativeUriComponents[1] : string.Empty,
			}.Uri;

			httpContext.Request.Scheme = requestUri.Scheme;
			httpContext.Request.Host = HostString.FromUriComponent(requestUri);
			httpContext.Request.Path = PathString.FromUriComponent(requestUri);
			httpContext.Request.QueryString = QueryString.FromUriComponent(requestUri);
			httpContext.Request.Query = new QueryCollection(QueryHelpers.ParseQuery(requestUri.Query));
			httpContext.Request.ContentType = requestInfo.ContentType;
			httpContext.Request.Method = requestInfo.Method.ToUpper();
			httpContext.Request.Protocol = _batchRequestOptions.DefaultProtocol;
			httpContext.Request.IsHttps = httpContext.Request.Scheme == Uri.UriSchemeHttps;

			httpContext.Response.Body = new MemoryStream();
			if (!string.IsNullOrEmpty(requestInfo.Body))
			{
				byte[] bodyData;
				if (requestInfo.Base64Encoded)
				{
					try
					{
						bodyData = Convert.FromBase64String(requestInfo.Body);
					}
					catch (FormatException)
					{
						throw new Base64FormatException()
						{
							Base64 = requestInfo.Body
						};
					}
				}
				else
				{
					bodyData = Encoding.UTF8.GetBytes(requestInfo.Body);
				}
				httpContext.Request.Body = new MemoryStream(bodyData);
			}

			return httpContext;
		}

		/// <summary>
		/// Creates a new not found result
		/// </summary>
		/// <returns>
		/// The fake not found result
		/// </returns>
		private static Task<HttpContext> GetNotFoundResult()
		{
			HttpContext httpContext = new DefaultHttpContext();
			httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
			return Task.FromResult(httpContext);
		}

		/// <summary>
		/// Checks whether the endpoint is matched the relative uri specified int the request info.
		/// </summary>
		/// <param name="routeEndpoint">The route endpoint.</param>
		/// <param name="requestInfo">The request information.</param>
		/// <param name="routeValueDictionary">The route value dictionary.</param>
		/// <returns>
		/// Whether the endpoint matches the request info relative uri.
		/// </returns>
		private static EndpointMatchResult EndpointMatches(RouteEndpoint routeEndpoint, RequestInfo requestInfo, out RouteValueDictionary routeValueDictionary)
		{
			if (!_templateMatcherCache.TryGetValue(routeEndpoint.RoutePattern.RawText, out TemplateMatcher templateMatcher))
			{
				RouteTemplate template = TemplateParser.Parse(routeEndpoint.RoutePattern.RawText);
				templateMatcher = new TemplateMatcher(template, GetDefaultRouteValues(template));
				_templateMatcherCache.TryAdd(routeEndpoint.RoutePattern.RawText, templateMatcher);
			}

			routeValueDictionary = new RouteValueDictionary();
			if (templateMatcher.TryMatch(new PathString(requestInfo.RelativeUri), routeValueDictionary))
			{
				// Check if the HTTP method matches
				string requestHttpMethod = requestInfo.Method.ToLower();

				HttpMethodMetadata httpMethodMetadata = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>();
				if (httpMethodMetadata == null && requestHttpMethod != HttpMethod.Get.ToString().ToLower())
				{ // Assume get if no metadata is found
					return EndpointMatchResult.NoMatch;
				}
				if (!httpMethodMetadata.HttpMethods.Any(httpMethod => httpMethod.ToLower() == requestHttpMethod))
				{ // Http method is not matching
					return EndpointMatchResult.NoMatch;
				}

				// Check if this endpoint is ignored, allowed takes precedence
				IgnoreForBatchRequestAttribute ignoreAttribute = routeEndpoint.Metadata.GetMetadata<IgnoreForBatchRequestAttribute>();
				AllowForBatchRequestAttribute allowAttribute = routeEndpoint.Metadata.GetMetadata<AllowForBatchRequestAttribute>();
				if (ignoreAttribute != null && allowAttribute == null)
				{
					return EndpointMatchResult.Ignored;
				}

				return EndpointMatchResult.Match;
			}
			return EndpointMatchResult.NoMatch;
		}

		/// <summary>
		/// Gets the default route values.
		/// </summary>
		/// <param name="parsedTemplate">The parsed template.</param>
		/// <returns>The default route values for the route template</returns>
		private static RouteValueDictionary GetDefaultRouteValues(RouteTemplate parsedTemplate)
		{
			RouteValueDictionary result = new RouteValueDictionary();
			foreach (TemplatePart parameter in parsedTemplate.Parameters)
			{
				if (parameter.DefaultValue != null)
				{
					result.Add(parameter.Name, parameter.DefaultValue);
				}
			}

			return result;
		}
	}
}
