using System.Collections.Generic;
using System.Linq;
using TomPIT.Data;
using TomPIT.DataProviders.Sql.Synchronization.Commands;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ExistingModel : IModelSchema
	{
		private List<IModelSchemaColumn> _columns = null;
		public List<IModelSchemaColumn> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<IModelSchemaColumn>();

				return _columns;
			}
		}

		public string Schema { get; set; }

		public string Name { get; set; }

		public string Type { get; set; }

		public bool Ignore { get; set; }

		public string Dependency { get; set; }

		public ObjectDescriptor Descriptor { get; private set; }

		public void Load(ISynchronizer owner)
		{
			Name = owner.Model.Name;
			Type = owner.Model.Type;
			Schema = owner.Model.SchemaName();

			Columns.AddRange(new Columns(owner).Execute());
			Descriptor = new SpHelp(owner).Execute();

			if (Columns.FirstOrDefault(f => string.Compare(f.Name, Descriptor.Identity.Identity, true) == 0) is ExistingColumn c)
				c.IsIdentity = true;

			foreach (var index in Descriptor.Indexes)
			{
				foreach (var column in index.Columns)
				{
					if (!(Columns.FirstOrDefault(f => string.Compare(column, f.Name, true) == 0) is ExistingColumn col))
						continue;

					switch (index.Type)
					{
						case IndexType.Index:
							col.IsIndex = true;
							break;
						case IndexType.Unique:
							col.IsIndex = true;
							col.IsUnique = true;
							break;
						case IndexType.PrimaryKey:
							col.IsPrimaryKey = true;
							break;
					}
				}
			}

			foreach (var constraint in Descriptor.Constraints)
			{
				switch (constraint.ConstraintType)
				{
					case ConstraintType.Default:
						if (Columns.FirstOrDefault(f => string.Compare(f.Name, constraint.Columns[0], true) == 0) is ExistingColumn column)
							column.DefaultValue = constraint.DefaultValue;
						break;
				}
			}
		}

		public List<ObjectIndex> Indexes
		{
			get
			{
				var result = new List<ObjectIndex>();

				foreach (var column in Columns)
				{
					var indexes = ResolveIndexes(column.Name);

					foreach (var index in indexes)
					{
						if (result.FirstOrDefault(f => string.Compare(f.Name, index.Name, true) == 0) == null)
							result.Add(index);
					}
				}

				return result;
			}
		}
		public List<ObjectIndex> ResolveIndexes(string column)
		{
			var result = new List<ObjectIndex>();

			foreach (var index in Descriptor.Indexes)
			{
				if (index.IsReferencedBy(column))
					result.Add(index);
			}

			return result;
		}
	}
}
