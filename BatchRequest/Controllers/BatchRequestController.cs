using BatchRequest.Abstractions;
using BatchRequest.Attributes;
using BatchRequest.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BatchRequest.Controllers
{
	[ApiController]
	[Route("/api/batch")]
	[IgnoreForBatchRequest]
	public class BatchRequestController : ControllerBase
	{
		/// <summary>
		/// The batch request options
		/// </summary>
		private readonly BatchRequestOptions _batchRequestOptions;
		/// <summary>
		/// The batch request service
		/// </summary>
		private readonly IBatchRequestService _batchRequestService;

		/// <summary>
		/// Initializes a new instance
		/// </summary>
		/// <param name="batchRequestOptions">The injected batch request options</param>
		public BatchRequestController(BatchRequestOptions batchRequestOptions, IBatchRequestService batchRequestService)
		{
			_batchRequestOptions = batchRequestOptions;
			_batchRequestService = batchRequestService;
		}

		[HttpPost]
		[Consumes("application/json")]
		[Produces("application/json")]
		[AllowForBatchRequest]
		public IActionResult BatchRequest(RequestInfo[] requestInfos)
		{
			if (!_batchRequestOptions.EnableEndpoint)
			{
				return NotFound();
			}

			if (_batchRequestService.Run(requestInfos, out IEnumerable<RequestResult> requestResults))
			{
				return Ok(requestResults);
			}
			return BadRequest(); ;
		}
	}
}
