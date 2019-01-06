using System;
using System.Collections.Generic;
using TomPIT.Ide;

namespace TomPIT.Design
{
	internal class Signature : ITextSignature
	{
		private List<ITextSignatureParameter> _parameters = null;

		public Type ReturnValue { get; set; }

		public string Name { get; set; }

		public List<ITextSignatureParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ITextSignatureParameter>();

				return _parameters;
			}
		}

		public string Text { get; set; }
		public string Language { get; set; }
	}
}
