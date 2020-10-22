using BatchRequest;
using BatchRequest.Abstractions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Extensions for the IServiceCollection
	/// </summary>
	public static class BatchRequestServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the batch requests with the default settings
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddBatchRequest(this IServiceCollection serviceCollection)
		{
			return AddBatchRequest(serviceCollection, null);
		}

		/// <summary>
		/// Adds the batch requests with default settings, optionally modified by the action
		/// </summary>
		/// <param name="serviceCollection">The service collection</param>
		/// <param name="batchRequestOptionsAction">The action to modify the default settings</param>
		/// <returns>The service collection</returns>
		public static IServiceCollection AddBatchRequest(this IServiceCollection serviceCollection, Action<BatchRequestOptions> batchRequestOptionsAction)
		{
			serviceCollection.AddRouting();
			serviceCollection.AddHttpContextAccessor();

			BatchRequestOptions batchRequestOptions = new BatchRequestOptions();
			if (batchRequestOptionsAction != null)
			{
				batchRequestOptionsAction.Invoke(batchRequestOptions);
			}

			BatchRequestOptionsDefaults.SetDefaults(batchRequestOptions);

			serviceCollection.AddSingleton(batchRequestOptions);
			serviceCollection.AddScoped<IBatchRequestService, BatchRequestService>();

			return serviceCollection;
		}
	}
}
