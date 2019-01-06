using System.Collections.Generic;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel
{
	public class ValidatingEventArguments : EventArguments
	{
		private List<string> _errors = null;

		public ValidatingEventArguments(IApplicationContext sender) : base(sender)
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
