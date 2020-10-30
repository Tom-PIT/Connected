using System.Collections.Generic;
using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class MergeResultRecord : DatabaseRecord
	{
		private List<MergeResultField> _fields = null;
		public List<MergeResultField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new List<MergeResultField>();

				return _fields;
			}
		}

		protected override void OnCreate()
		{
			for (var i = 1; i < Reader.FieldCount; i++)
			{
				Fields.Add(new MergeResultField
				{
					Name = Reader.GetName(i),
					Value = Reader.GetValue(i)
				});
			}
		}
	}
}