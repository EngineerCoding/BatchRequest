namespace BatchRequest.Models
{
	/// <summary>
	/// The object which defines a result of an internal batch request
	/// </summary>
	public class RequestResult
	{
		/// <summary>
		/// The status code of the result
		/// </summary>
		public int StatusCode { get; set; }

		/// <summary>
		/// The content type of the response
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// The body of the result, base64 encoded when <see cref="Base64Encoded"/> is set to true
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Whether the body is base64 encoded
		/// </summary>
		public bool Base64Encoded { get; set; }
	}
}
