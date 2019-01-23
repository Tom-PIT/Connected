using System;
using System.Data;
using TomPIT.Application.Data;
using TomPIT.ComponentModel.Data;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Designers
{
	internal class DataSource : DataSourceDesigner
	{
		public DataSource(IEnvironment environment, ComponentElement element) : base(environment, element)
		{
		}

		protected override void SetAttributes(Guid connection, string commandText, CommandType commandType)
		{
			Configuration.Connection = connection;
			Configuration.CommandText = commandText;
			Configuration.CommandType = commandType;
		}

		protected override IBoundField CreateField(string name, DataType dataType)
		{
			return new BoundField
			{
				DataType = dataType,
				Name = name
			};
		}

		protected override ComponentModel.Data.IDataParameter CreateParameter(string name, DataType dataType, bool isNullable)
		{
			return new Parameter
			{
				DataType = dataType,
				Name = name,
				IsNullable = isNullable,
				NullMapping = isNullable
			};
		}

		private Data.DataSource Configuration { get { return DataSource as Data.DataSource; } }


	}
}
