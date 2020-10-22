namespace BatchRequest.Models
{
	/// <summary>
	/// The object which is used to identify a single request
	/// </summary>
	public class RequestInfo
	{
		/// <summary>
		/// The relative URI of the request
		/// </summary>
		public string RelativeUri { get; set; }

		/// <summary>
		/// The request method to execute for this <see cref="RelativeUri"/>
		/// </summary>
		public string Method { get; set; } = HttpMethod.Get.ToString();

		/// <summary>
		/// The content type of this request
		/// </summary>
		public string ContentType { get; set; }

		/// <summary>
		/// The body of this request. When binary data has to be supplied, encode this with base64
		/// and set <see cref="Base64Encoded"/> to true
		/// </summary>
		public string Body { get; set; }

		/// <summary>
		/// Whether the body is encoded with base 64
		/// </summary>
		public bool Base64Encoded { get; set; }
	}
}
