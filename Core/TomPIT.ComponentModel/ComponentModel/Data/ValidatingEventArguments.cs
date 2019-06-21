using System.Collections.Generic;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class ValidatingEventArguments : DataModelContext
	{
		private List<string> _errors = null;

		public ValidatingEventArguments(IExecutionContext sender) : base(sender)
		{
		}

		public List<string> ValidationErrors
		{
			get
			{
				if (_errors == null)
					_errors = new List<string>();

				return _errors;
			}
		}

	}
}
