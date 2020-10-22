using System;

namespace BatchRequest
{
	/// <summary>
	/// Options for this ASP.NET Core extension
	/// </summary>
	public class BatchRequestOptions
	{
		/// <summary>
		/// Whether to add a controller exposed at <pre>/api/batch</pre>
		/// </summary>
		public bool EnableEndpoint { get; set; } = true;

		/// <summary>
		/// The host used for making requests to the internal endpoints
		/// </summary>
		public Uri RequestHost { get; set; }

		/// <summary>
		/// The protocol used for making requests to the internal endpoints
		/// </summary>
		public string DefaultProtocol { get; set; }
	}
}
