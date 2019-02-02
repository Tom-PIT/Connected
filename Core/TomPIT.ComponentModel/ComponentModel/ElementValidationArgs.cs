using System;
using System.Collections.Generic;
using TomPIT.Services;

namespace TomPIT.ComponentModel
{
	public class ElementValidationArgs : EventArgs
	{
		private List<string> _errors = null;

		public ElementValidationArgs(IExecutionContext context)
		{
			Context = context;
		}

		public IExecutionContext Context { get; }
		public List<string> Errors
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
