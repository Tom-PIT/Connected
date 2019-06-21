using Newtonsoft.Json.Linq;
using TomPIT.Data;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Data
{
	public class TransactionExecutedArguments : DataModelContext
	{
		private JObject _returnValues = null;

		internal TransactionExecutedArguments(IExecutionContext sender, JObject returnValues) : base(sender)
		{
			ReturnValues = returnValues;
		}

		public JObject ReturnValues
		{
			get
			{
				if (_returnValues == null)
					_returnValues = new JObject();

				return _returnValues;
			}
			private set
			{
				_returnValues = value;
			}
		}
	}
}
