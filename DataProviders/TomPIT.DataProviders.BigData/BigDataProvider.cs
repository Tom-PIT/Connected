using System;
using System.Data;
using TomPIT.Data;
using TomPIT.Data.DataProviders;
using TomPIT.Data.DataProviders.Design;

namespace TomPIT.DataProviders.BigData
{
	[SchemaBrowser("TomPIT.DataProviders.BigData.Design.Browser, TomPIT.DataProviders.BigData")]
	public class BigDataProvider : DataProviderBase<DataConnection>
	{
		public BigDataProvider() : base("BigData", new Guid("BFEE9DDE-3721-4168-B902-DBC72E63EE22"))
		{

		}

		public override IDataConnection OpenConnection(string connectionString, ConnectionBehavior behavior)
		{
			return new DataConnection(this, connectionString, behavior);
		}

		protected override void SetupParameters(IDataCommandDescriptor command, IDbCommand cmd)
		{
			if (cmd.Parameters.Count > 0)
			{
				foreach (BigDataParameter i in cmd.Parameters)
					i.Value = DBNull.Value;
			}
			else
			{
				BigDataParameter rv = null;

				foreach (var i in command.Parameters)
				{
					var p = new BigDataParameter
					{
						ParameterName = i.Name,
						DbType = i.DataType
					};

					if (i.Direction == ParameterDirection.ReturnValue)
						p.Direction = ParameterDirection.ReturnValue;

					cmd.Parameters.Add(p);

					if (i.Direction == ParameterDirection.ReturnValue && rv == null)
						rv = p;
				}
			}
		}

		protected override object GetParameterValue(IDbCommand command, string parameterName)
		{
			var cmd = command as BigDataCommand;

			return cmd.Parameters[parameterName].Value;
		}

		protected override void SetParameterValue(IDbCommand command, string parameterName, object value)
		{
			var cmd = command as BigDataCommand;

			cmd.Parameters[parameterName].Value = value;
		}
	}
}
