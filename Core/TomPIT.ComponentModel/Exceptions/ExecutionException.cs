using System;
using TomPIT.Exceptions;
using TomPIT.Services;

namespace TomPIT.Exceptions
{
	public class ExecutionException : TomPITException
	{
		public ExecutionException(string source, string message)
		: base(message)
		{
			Source = source;
		}

		public ExecutionException(string message)
		: base(message)
		{

		}

		public ExecutionException(string message, Exception inner)
			: base(message, inner)
		{

		}

		public static ExecutionException Create(IExecutionContext sender, string message, IExecutionContextState state)
		{
			return new ExecutionException(message)
			{
				Source = sender == null ? "Shell" : sender.Identity.Authority,
				AuthorityId = sender == null ? string.Empty : sender.Identity.AuthorityId,
				Id = sender == null ? string.Empty : sender.Identity.ContextId,
				Descriptor = state
			};
		}

		public string AuthorityId { get; set; }
		public string Id { get; set; }

		public IExecutionContextState Descriptor { get; set; }

		public static ExecutionException ParameterExpected(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrDataParameterExpected, name), state);
		}

		public static ExecutionException ParameterConversion(IExecutionContext sender, IExecutionContextState state, string name, string valueType, string expectedType)
		{
			return Create(sender, string.Format("{0} ({1}, {2}, {3})", SR.ErrParameterValueConversion, name, valueType, expectedType), state);
		}

		public static ExecutionException InvalidParameterDataType(IExecutionContext sender, IExecutionContextState state, string name, string valueType, string expectedType)
		{
			return Create(sender, string.Format("{0} ({1}, {2}, {3})", SR.ErrInvalidParameterDataType, name, valueType, expectedType), state);
		}

		public static ExecutionException TimezoneParametersSupportedOnDatesOnly(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrTimezoneParametersSupportedOnDatesOnly, name), state);
		}

		public static ExecutionException ParameterNotDefined(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrParameterNotDefined, name), state);
		}

		public static ExecutionException ParameterValueNotSet(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrParameterValueNotSet, name), state);
		}

		public static ExecutionException ParameterNameNotSet(IExecutionContext sender, IExecutionContextState state)
		{
			return Create(sender, SR.ErrParameterNameNotSet, state);
		}

		public static ExecutionException InvalidMicroServiceQualifier(IExecutionContext sender, IExecutionContextState state, string microService)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrInvalidMicroServiceQualifier, microService), state);
		}

		public static ExecutionException CannotResolveMicroService(IExecutionContext sender, IExecutionContextState state)
		{
			return Create(sender, SR.ErrCannotResolveMicroService, state);
		}

		public static ExecutionException ConnectionDataProviderNotFound(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, name), state);
		}

		public static ExecutionException ConnectionDataProviderNotSet(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, name), state);
		}

		public static ExecutionException ConnectionNotFound(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionNotFound, name), state);
		}

		public static ExecutionException ConnectionNotSet(IExecutionContext sender, IExecutionContextState state, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionNotSet, name), state);
		}

		public static ExecutionException InvalidQualifier(IExecutionContext sender, IExecutionContextState state, string qualifier, string expected)
		{
			var invalid = SR.ErrInvalidQualifier;
			var exp = SR.ErrInvalidQualifierExpected;

			return Create(sender, string.Format("{0} ({1}). {2}: {3}.", invalid, qualifier, exp, expected), state);
		}
	}
}
