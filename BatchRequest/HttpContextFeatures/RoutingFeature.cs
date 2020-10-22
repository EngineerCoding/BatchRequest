using Microsoft.AspNetCore.Routing;

namespace BatchRequest.HttpContextFeatures
{
	internal class RoutingFeature : IRoutingFeature
	{
		public RouteData RouteData { get; set; }

		public RoutingFeature(RouteValueDictionary routeValues)
		{
			RouteData = new RouteData(routeValues);
		}
	}
}
