using System;
using TomPIT.Reflection;

namespace TomPIT.Exceptions
{
	public class ConcurrencyException : RuntimeException
	{
		public ConcurrencyException(Type model, string operation)
		{
			Model = model;
			Operation = operation;
			Source = SR.ErrConcurrencySource;
		}

		private string Operation { get; }
		private Type Model { get; }
		public override string Message => $"{SR.ErrConcurrencyMessage} ({Model.ShortName()}/{Operation})";
	}
}
