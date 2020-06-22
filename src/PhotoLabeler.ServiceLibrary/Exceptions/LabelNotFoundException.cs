using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoLabeler.ServiceLibrary.Exceptions
{

	[Serializable]
	public class LabelNotFoundException : Exception
	{
		public LabelNotFoundException() { }
		public LabelNotFoundException(string message) : base(message) { }
		public LabelNotFoundException(string message, Exception inner) : base(message, inner) { }
		protected LabelNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
