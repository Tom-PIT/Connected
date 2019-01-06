using System;
using TomPIT.Exceptions;

namespace TomPIT
{
	public class IdeException : TomPITException
	{
		private const string Reflector = "Reflector";

		public IdeException(string message)
			: base(message)
		{

		}

		public IdeException(string message, Exception inner)
			: base(message, inner)
		{

		}

		private static IdeException Management(string message, int eventId, string source)
		{
			return new IdeException(message)
			{
				Source = source,
				Event = eventId
			};
		}

		public static IdeException CannotResolveType(string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrTypeNull, type), 0, Reflector);
		}
		//todo:review exception below

		public static IdeException TransactionNotCommited(object sender, int eventId)
		{
			return Management(SR.ErrTransactionNotCommited, eventId, sender.GetType().Name);
		}

		public static IdeException NoTransactionHandler(object sender, int eventId)
		{
			return Management(SR.ErrNoTransactionHandler, eventId, sender.GetType().Name);
		}

		public static IdeException MissingRemoveMethod(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrMissingRemoveMethod, type), eventId, sender.GetType().Name);
		}

		public static IdeException MissingIndexer(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrMissingIndexer, type), eventId, sender.GetType().Name);
		}

		public static IdeException MissingInsertMethod(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrMissingInsertMethod, type), eventId, sender.GetType().Name);
		}

		public static IdeException MissingClearMethod(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrMissingClearMethod, type), eventId, sender.GetType().Name);
		}

		public static IdeException MissingAddMethod(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrMissingAddMethod, type), eventId, sender.GetType().Name);
		}

		public static IdeException CannotCreateInstance(object sender, int eventId, Type type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrCannotCreateInstance, type == null ? "null" : type.Name), eventId, sender.GetType().Name);
		}

		public static IdeException CannotCreateInstance(object sender, int eventId, string type)
		{
			return Management(string.Format("{0} ({1})", SR.ErrCannotCreateInstance, type), eventId, sender.GetType().Name);
		}

		public static IdeException CannotFindConfiguration(object sender, int eventId, string componentType)
		{
			return Management(string.Format("{0} ({1})", SR.ErrCannotFindConfiguration, componentType), eventId, sender.GetType().Name);
		}

		public static IdeException NoPropertyDesigner(object sender, int eventId, string property)
		{
			return Management(string.Format("{0} ({1})", SR.ErrNoPropertyDesigner, property), eventId, sender.GetType().Name);
		}

		public static IdeException InvalidPropertyDesigner(object sender, int eventId, string property, string designer)
		{
			return Management(string.Format("{0} ({1}, {2})", SR.ErrInvalidPropertyDesigner, property, designer), eventId, sender.GetType().Name);
		}

		public static IdeException DataProviderNotFound(object sender, int eventId, string connection)
		{
			return Management(string.Format("{0} ({1})", SR.ErrDataProviderNotFound, connection), eventId, sender.GetType().Name);
		}

		public static IdeException InvalidSection(object sender, int eventId)
		{
			return Management(SR.ErrInvalidSection, eventId, sender.GetType().Name);
		}

		public static IdeException ConversionError(object sender, int eventId, string property, string value, Type type)
		{
			return Management(string.Format("{0} ({1}.{2}, {3})", SR.ErrConversion, property, value, type.Name), eventId, sender.GetType().Name);
		}

		public static IdeException PropertyNotFound(object sender, int eventId, object instance, string property)
		{
			return Management(string.Format("{0} ({1}.{2})", SR.ErrPropertyNotFound, instance == null ? string.Empty : instance.GetType().Name, property), eventId, sender.GetType().Name);
		}

		public static IdeException ValidationFailed(object sender, int eventId, string message)
		{
			return Management(string.Format("{0} {1}", SR.ErrValidationFailed, message), eventId, sender.GetType().Name);
		}

		public static IdeException NoDesigner(object sender, int eventId)
		{
			return Management(SR.ErrNoDesigner, eventId, sender.GetType().Name);
		}

		public static IdeException InvalidPath(object sender, int eventId)
		{
			return Management(SR.ErrInvalidPath, eventId, sender.GetType().Name);
		}

		public static IdeException PathNotSet(object sender, int eventId)
		{
			return Management(SR.ErrPathNotSet, eventId, sender.GetType().Name);
		}

		public static IdeException ExpectedParameter(object sender, int eventId, string parameter)
		{
			return Management(string.Format("{0} ({1})", SR.ErrExpectedParameter, parameter), eventId, sender.GetType().Name);
		}

		public static IdeException DesignerActionNotSupported(object sender, int eventId, string action)
		{
			return Management(string.Format("{0} ({1})", SR.ErrActionNotSupported, action), eventId, sender.GetType().Name);
		}
	}
}