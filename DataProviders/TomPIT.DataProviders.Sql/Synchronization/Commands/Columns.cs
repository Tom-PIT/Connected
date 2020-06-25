using System.Collections.Generic;
using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class Columns : SynchronizationQuery<List<IModelSchemaColumn>>
	{
		public Columns(ISynchronizer owner) : base(owner)
		{
		}

		protected override List<IModelSchemaColumn> OnExecute()
		{
			var result = new List<IModelSchemaColumn>();
			var rdr = Owner.CreateCommand(CommandText).ExecuteReader();

			while (rdr.Read())
			{
				var column = new ExistingColumn
				{
					IsNullable = string.Compare(rdr.GetValue("IS_NULLABLE", string.Empty), "NO", true) == 0 ? false : true,
					DataType = DataTypeUtils.ToDbType(rdr.GetValue("DATA_TYPE", string.Empty)),
					MaxLength = rdr.GetValue("CHARACTER_MAXIMUM_LENGTH", 0),
					Name = rdr.GetValue("COLUMN_NAME", string.Empty),
				};

				result.Add(column);
			}

			rdr.Close();

			return result;
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{Model.SchemaName()}' AND TABLE_NAME = '{Owner.Model.Name}'");

				return text.ToString();
			}
		}
	}
}
