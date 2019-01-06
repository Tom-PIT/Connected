using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TomPIT.ComponentModel.DataProviders;

namespace TomPIT.Runtime.ApplicationContextServices
{
	internal class DataCommandDescriptor : IDataCommandDescriptor
	{
		private List<ICommandParameter> _parameters = null;
		public string CommandText { get; set; }
		public int CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public string ConnectionString { get; set; }
		public object ReturnValue { get; set; }

		public List<ICommandParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ICommandParameter>();

				return _parameters;
			}
		}

		public void SetParameterValue(string name, object value)
		{
			var param = Parameters.FirstOrDefault(f => string.Compare(f.Name, name, true) == 0);

			if (param == null)
				throw new Exception("Parameter not found");

			if (!(param is CommandParameter cp))
				throw new Exception("Parameter not writable");

			cp.Value = value;
		}
	}
}
