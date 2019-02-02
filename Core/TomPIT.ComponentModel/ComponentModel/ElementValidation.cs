using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	internal delegate void ElementValidationHandler(object sender, ElementValidationArgs e);

	internal class ElementValidation : IElementValidation
	{
		private bool _validated = false;

		private List<string> _validationErrors = null;

		public event ElementValidationHandler Validate;

		public bool IsValid(IExecutionContext context)
		{
			if (!_validated)
			{
				_validated = true;

				Errors.Clear();

				var args = new ElementValidationArgs(context);

				Validate?.Invoke(this, args);

				if (args.Errors.Count > 0)
					Errors.AddRange(args.Errors);
			}

			return Errors.Count == 0;
		}

		public List<string> ValidationErrors()
		{
			return Errors;
		}

		private List<string> Errors
		{
			get
			{
				if (_validationErrors == null)
					_validationErrors = new List<string>();

				return _validationErrors;
			}
		}
	}
}
