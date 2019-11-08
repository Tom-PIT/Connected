using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public class SignatureInformation : ISignatureInformation
	{
		private List<IParameterInformation> _parameters = null;

		public string Documentation { get; set; }

		public string Label { get; set; }

		public List<IParameterInformation> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<IParameterInformation>();

				return _parameters;
			}
		}
	}
}
