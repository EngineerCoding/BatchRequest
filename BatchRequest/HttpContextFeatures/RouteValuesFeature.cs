using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace BatchRequest.HttpContextFeatures
{
    internal class RouteValuesFeature : IRouteValuesFeature
    {
        public RouteValuesFeature(RouteValueDictionary routeValues)
        {
            this.RouteValues = routeValues;
        }

        public RouteValueDictionary RouteValues { get; set; }
    }
}
