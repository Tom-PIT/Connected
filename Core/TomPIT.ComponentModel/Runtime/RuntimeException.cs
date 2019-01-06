using System;
using TomPIT.Exceptions;

namespace TomPIT.Runtime
{
	public class RuntimeException : TomPITException
	{
		public RuntimeException(string source, string message)
		: base(message)
		{
			Source = source;
		}

		public RuntimeException(string message)
		: base(message)
		{

		}

		public RuntimeException(string message, Exception inner)
			: base(message, inner)
		{

		}

		public static RuntimeException Create(IApplicationContext sender, string message, IExecutionContext descriptor)
		{
			return new RuntimeException(message)
			{
				Source = sender == null ? "Shell" : sender.Identity.Authority,
				AuthorityId = sender == null ? string.Empty : sender.Identity.AuthorityId,
				Id = sender == null ? string.Empty : sender.Identity.ContextId,
				Descriptor = descriptor
			};
		}

		public string AuthorityId { get; set; }
		public string Id { get; set; }

		public IExecutionContext Descriptor { get; set; }

		public static RuntimeException ParameterExpected(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrDataParameterExpected, name), descriptor);
		}

		public static RuntimeException ParameterConversion(IApplicationContext sender, IExecutionContext descriptor, string name, string valueType, string expectedType)
		{
			return Create(sender, string.Format("{0} ({1}, {2}, {3})", SR.ErrParameterValueConversion, name, valueType, expectedType), descriptor);
		}

		public static RuntimeException InvalidParameterDataType(IApplicationContext sender, IExecutionContext descriptor, string name, string valueType, string expectedType)
		{
			return Create(sender, string.Format("{0} ({1}, {2}, {3})", SR.ErrInvalidParameterDataType, name, valueType, expectedType), descriptor);
		}

		public static RuntimeException TimezoneParametersSupportedOnDatesOnly(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrTimezoneParametersSupportedOnDatesOnly, name), descriptor);
		}

		public static RuntimeException ParameterNotDefined(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrParameterNotDefined, name), descriptor);
		}

		public static RuntimeException ParameterValueNotSet(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrParameterValueNotSet, name), descriptor);
		}

		public static RuntimeException ParameterNameNotSet(IApplicationContext sender, IExecutionContext descriptor)
		{
			return Create(sender, SR.ErrParameterNameNotSet, descriptor);
		}

		public static RuntimeException InvalidMicroServiceQualifier(IApplicationContext sender, IExecutionContext descriptor, string microService)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrInvalidMicroServiceQualifier, microService), descriptor);
		}

		public static RuntimeException CannotResolveMicroService(IApplicationContext sender, IExecutionContext descriptor)
		{
			return Create(sender, SR.ErrCannotResolveMicroService, descriptor);
		}

		public static RuntimeException ConnectionDataProviderNotFound(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotFound, name), descriptor);
		}

		public static RuntimeException ConnectionDataProviderNotSet(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionDataProviderNotSet, name), descriptor);
		}

		public static RuntimeException ConnectionNotFound(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionNotFound, name), descriptor);
		}

		public static RuntimeException ConnectionNotSet(IApplicationContext sender, IExecutionContext descriptor, string name)
		{
			return Create(sender, string.Format("{0} ({1})", SR.ErrConnectionNotSet, name), descriptor);
		}

		public static RuntimeException InvalidQualifier(IApplicationContext sender, IExecutionContext descriptor, string qualifier, string expected)
		{
			var invalid = SR.ErrInvalidQualifier;
			var exp = SR.ErrInvalidQualifierExpected;

			return Create(sender, string.Format("{0} ({1}). {2}: {3}.", invalid, qualifier, exp, expected), descriptor);
		}
	}
}
