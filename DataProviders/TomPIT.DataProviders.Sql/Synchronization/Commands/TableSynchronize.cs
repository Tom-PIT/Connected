using System.Linq;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableSynchronize : TableTransaction
	{
		private ExistingModel _existingSchema = null;
		public TableSynchronize(ISynchronizer owner) : base(owner)
		{
		}

		private bool TableExists { get; set; }

		protected override void OnExecute()
		{
			TableExists = new TableExists(Owner).Execute();

			if (TableExists)
			{
				Owner.ExistingModel = ExistingModel;

				if (ShouldRecreate)
					new TableRecreate(Owner, ExistingModel).Execute();
				else if (ShouldAlter)
					new TableAlter(Owner, ExistingModel).Execute();
			}
			else
				new TableCreate(Owner, false).Execute();
		}

		private bool ShouldAlter => !Model.Equals(ExistingModel);

		private ExistingModel ExistingModel
		{
			get
			{
				if (_existingSchema == null)
				{
					_existingSchema = new ExistingModel();
					_existingSchema.Load(Owner);
				}

				return _existingSchema;
			}
		}
		private bool ShouldRecreate => HasIdentityChanged || HasColumnMetadataChanged;

		private bool HasIdentityChanged
		{
			get
			{
				foreach (var column in Model.Columns)
				{
					var existing = ExistingModel.Columns.FirstOrDefault(f => string.Compare(f.Name, column.Name, true) == 0);

					if (existing == null)
						return true;

					if (existing.IsIdentity != column.IsIdentity)
						return true;
				}

				foreach (var existing in ExistingModel.Columns)
				{
					var column = Model.Columns.FirstOrDefault(f => string.Compare(f.Name, existing.Name, true) == 0);

					if (column == null && existing.IsIdentity)
						return true;
					else if (column != null && column.IsIdentity != existing.IsIdentity)
						return true;
				}

				return false;
			}
		}

		private bool HasColumnMetadataChanged
		{
			get
			{
				foreach (var existing in ExistingModel.Columns)
				{
					var column = Model.Columns.FirstOrDefault(f => string.Compare(f.Name, existing.Name, true) == 0);

					if (column == null)
						continue;

					if (column.DataType != existing.DataType
						|| column.MaxLength != existing.MaxLength
						|| column.IsNullable != existing.IsNullable
						|| column.IsVersion != existing.IsVersion
						|| column.Precision != existing.Precision
						|| column.Scale != existing.Scale
						|| column.DateKind != existing.DateKind
						|| column.DatePrecision != existing.DatePrecision)
						return true;
				}

				return false;
			}
		}
	}
}
