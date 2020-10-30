using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class TableAlter : TableSynchronize
	{
		public TableAlter(ISynchronizer owner, ExistingModel model) : base(owner)
		{
			Existing = model;
		}

		private ExistingModel Existing { get; }

		protected override void OnExecute()
		{
			var dropped = new List<ObjectIndex>();

			foreach (var index in Existing.Indexes)
			{
				if (!ColumnsMatched(index))
				{
					new IndexDrop(Owner, index).Execute();
					dropped.Add(index);
				}
			}

			foreach (var drop in dropped)
				Existing.Indexes.Remove(drop);

			foreach (var existingColumn in Existing.Columns)
			{
				var column = Model.Columns.FirstOrDefault(f => string.Compare(f.Name, existingColumn.Name, true) == 0);

				if (column == null)
					new ColumnDrop(Owner, existingColumn, Existing).Execute();
				else
					new ColumnAlter(Owner, column, Existing, existingColumn).Execute();
			}

			var indexes = ParseIndexes(Model);

			foreach (var index in indexes)
			{
				if (!IndexExists(index))
					new IndexCreate(Owner, index).Execute();
			}
		}

		private bool IndexExists(IndexDescriptor index)
		{
			var existingIndexes = Existing.Indexes.Where(f => f.Type != IndexType.PrimaryKey);

			foreach (var existingIndex in existingIndexes)
			{
				if (index.Unique && existingIndex.Type != IndexType.Unique)
					continue;

				if (!index.Unique && existingIndex.Type == IndexType.Unique)
					continue;

				var cols = index.Columns.OrderBy(f => f);
				var existingCols = existingIndex.Columns.OrderBy(f => f);

				if (cols.Count() != existingCols.Count())
					continue;

				for (var i = 0; i < cols.Count(); i++)
				{
					if (string.Compare(cols.ElementAt(i), existingCols.ElementAt(i), true) != 0)
						break;
				}

				return true;
			}

			return false;
		}

		private bool ColumnsMatched(ObjectIndex index)
		{
			if (index.Columns.Count == 1)
				return ColumnMatched(index);

			var indexGroup = string.Empty;
			var columns = new List<IModelSchemaColumn>();

			foreach (var column in Model.Columns)
			{
				if (index.Columns.Contains(column.Name, StringComparer.OrdinalIgnoreCase))
				{
					if (string.IsNullOrWhiteSpace(column.IndexGroup))
						return false;

					if (string.Compare(indexGroup, column.IndexGroup, true) != 0)
						return false;

					if (string.IsNullOrWhiteSpace(indexGroup))
						indexGroup = column.IndexGroup;

					columns.Add(column);
				}
			}

			foreach (var column in Model.Columns)
			{
				if (string.Compare(column.IndexGroup, indexGroup, true) == 0 && !columns.Contains(column) && column.IsIndex)
					columns.Add(column);
			}

			if (index.Columns.Count != columns.Count)
				return false;

			foreach (var column in columns.OrderBy(f => f.Name))
			{
				if (!index.Columns.Contains(column.Name, StringComparer.OrdinalIgnoreCase))
					return false;
			}

			return true;
		}

		private bool ColumnMatched(ObjectIndex index)
		{
			var column = Model.Columns.FirstOrDefault(f => string.Compare(f.Name, index.Columns[0], true) == 0);

			if (column == null)
				return false;

			if (!column.IsIndex)
				return false;

			if (index.Type == IndexType.Unique && !column.IsUnique)
				return false;

			return true;
		}
	}
}
