using BatchRequest.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BatchRequest.Abstractions
{
	/// <summary>
	/// The service used for invoking batch requests by transforming RequestInfo's to functions which actually
	/// invoke the endpoint, transforming the HttpContext objects to RequestResults and invoking a complete
	/// flow for a single batch request.
	/// </summary>
	public interface IBatchRequestService
	{
		bool Run(IEnumerable<RequestInfo> requestInfos, out IEnumerable<RequestResult> requestResults);

		/// <summary>
		/// Transforms all <seealso cref="RequestInfo"/> objects to a function which returns the <seealso cref="HttpContext"/>
		/// with the response.
		/// </summary>
		/// <param name="requestInfos">The info's to transform</param>
		/// <returns>
		/// A function which returns a task which executes the request info. This method
		/// will return null when some validation fails.
		/// </returns>
		IEnumerable<Func<Task<HttpContext>>> TransformToInternalRequests(IEnumerable<RequestInfo> requestInfos);

		/// <summary>
		/// Transforms a <seealso cref="HttpContext"/> to a <seealso cref="RequestResult"/> object
		/// </summary>
		/// <param name="httpContexts">The contexts to transform</param>
		/// <returns>The result objects</returns>
		IEnumerable<RequestResult> TransformToRequestResults(IEnumerable<HttpContext> httpContexts);
	}
}
