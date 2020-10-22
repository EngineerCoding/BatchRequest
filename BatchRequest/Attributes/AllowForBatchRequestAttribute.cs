using System;

namespace BatchRequest.Attributes
{
	/// <summary>
	/// An attribute to allow a method for batch requests, when the enclosing controller
	/// has the <seealso cref="IgnoreForBatchRequestAttribute"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	class AllowForBatchRequestAttribute : Attribute
	{
	}
}
