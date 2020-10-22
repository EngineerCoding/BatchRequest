using System;

namespace BatchRequest
{
	public static class BatchRequestOptionsDefaults
	{
		/// <summary>
		/// The default host
		/// </summary>
		public static readonly Uri BatchRequestHost = new Uri("https://batchrequest");
		/// <summary>
		/// The default protocol
		/// </summary>
		public static readonly string DefaultProtocol = "BatchRequest";

		/// <summary>
		/// Sets default values on the options
		/// </summary>
		/// <param name="batchRequestOptions"></param>
		internal static void SetDefaults(BatchRequestOptions batchRequestOptions)
		{
			if (batchRequestOptions.RequestHost == null)
			{
				batchRequestOptions.RequestHost = BatchRequestHost;
			}

			if (string.IsNullOrEmpty(batchRequestOptions.DefaultProtocol))
			{
				batchRequestOptions.DefaultProtocol = DefaultProtocol;
			}
		}
	}
}
