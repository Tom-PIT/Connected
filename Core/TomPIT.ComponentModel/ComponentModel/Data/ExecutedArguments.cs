using Newtonsoft.Json.Linq;
using TomPIT.Services;

namespace TomPIT.ComponentModel.Data
{
	public class ExecutedArguments : EventArguments
	{
		private JObject _returnValues = null;

		internal ExecutedArguments(IExecutionContext sender, JObject data, JObject returnValues) : base(sender)
		{
			Data = data;
			ReturnValues = returnValues;
		}

		public JObject Data { get; set; }

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
