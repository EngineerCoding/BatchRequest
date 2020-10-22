using System;
using System.Runtime.Serialization;

namespace BatchRequest.Exceptions
{
	[Serializable]
	internal class Base64FormatException : FormatException
	{
		public string Base64 { get; set; }

		public Base64FormatException()
		{
		}

		public Base64FormatException(string message) : base(message)
		{
		}

		public Base64FormatException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected Base64FormatException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
