using Newtonsoft.Json.Linq;
using TomPIT.Runtime;

namespace TomPIT.ComponentModel.Data
{
	public class TransactionPreparingArguments : CancelEventArguments
	{
		private JObject _arguments = null;

		public TransactionPreparingArguments(IApplicationContext sender, JObject arguments) : base(sender)
		{
			Arguments = arguments;
		}

		public JObject Arguments
		{
			get
			{
				if (_arguments == null)
					_arguments = new JObject();

				return _arguments;
			}
			private set
			{
				_arguments = value;
			}
		}
	}
}
