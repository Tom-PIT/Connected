using System;
using System.Collections.Generic;
using TomPIT.Middleware;

namespace TomPIT.Design.Validation
{
	public class ElementValidationArgs : EventArgs
	{
		private List<IValidationMessage> _messages = null;

		public ElementValidationArgs(IMiddlewareContext context)
		{
			Context = context;
		}

		public IMiddlewareContext Context { get; }
		public List<IValidationMessage> Messages
		{
			get
			{
				if (_messages == null)
					_messages = new List<IValidationMessage>();

				return _messages;
			}
		}

		public void Warning(string message)
		{
			Messages.Add(new ValidationMessage
			{
				Type = ValidationMessageType.Warning,
				Message = message
			});
		}

		public void Error(string message)
		{
			Messages.Add(new ValidationMessage
			{
				Type = ValidationMessageType.Error,
				Message = message
			});
		}

		public void Suggestion(string message)
		{
			Messages.Add(new ValidationMessage
			{
				Type = ValidationMessageType.Suggestion,
				Message = message
			});
		}
	}
}
