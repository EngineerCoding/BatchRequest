using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace BatchRequest.HttpContextFeatures
{
	internal class EndpointFeature : IEndpointFeature
	{
		public Endpoint Endpoint { get; set; }

		public EndpointFeature(Endpoint endpoint)
		{
			Endpoint = endpoint;
		}
	}
}
