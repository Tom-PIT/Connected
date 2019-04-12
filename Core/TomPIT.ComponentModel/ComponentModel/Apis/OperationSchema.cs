using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationSchema
	{
		private List<SchemaParameter> _parameters = null;
		private JObject _returnValue = null;

		public List<SchemaParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<SchemaParameter>();

				return _parameters;
			}
		}

		public JObject ReturnValue
		{
			get
			{
				if (_returnValue == null)
					_returnValue = new JObject();

				return _returnValue;
			}
		}
	}
}
