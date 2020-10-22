using System;

namespace BatchRequest.Attributes
{
	/// <summary>
	/// An attribute which indicates that the methods in a controller or controller method cannot be
	/// executed through a batch request.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class IgnoreForBatchRequestAttribute : Attribute
	{
	}
}
