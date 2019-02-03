using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	internal delegate void ElementValidationHandler(object sender, ElementValidationArgs e);

	internal class ElementValidation : IElementValidation
	{
		public event ElementValidationHandler Validating;

		private bool _validated = false;
		private List<IValidationMessage> _validationErrors = null;

		public bool Validate(IExecutionContext context)
		{
			if (!_validated)
			{
				_validated = true;

				Messages.Clear();

				var args = new ElementValidationArgs(context);

				Validating?.Invoke(this, args);

				if (args.Messages.Count > 0)
					Messages.AddRange(args.Messages);
			}

			return Messages.Count(f => f.Type == ValidationMessageType.Error) == 0;
		}

		[JsonIgnore]
		public bool IsValid { get; }

		public List<IValidationMessage> ValidationMessages()
		{
			return Messages;
		}

		[JsonIgnore]
		private List<IValidationMessage> Messages
		{
			get
			{
				if (_validationErrors == null)
					_validationErrors = new List<IValidationMessage>();

				return _validationErrors;
			}
		}
	}
}
