namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ExistingIndex
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Keys { get; set; }

		public bool IsConstraint
		{
			get
			{
				var tokens = Description.Split(',');

				foreach (var token in tokens)
				{
					if (string.Compare(token.Trim(), "unique", true) == 0
						|| token.Trim().StartsWith("primary key "))
						return true;
				}

				return false;
			}
		}

		public bool IsReferencedBy(string column)
		{
			var tokens = Keys.Split(',');

			foreach (var token in tokens)
			{
				if (string.Compare(token.Trim(), column, true) == 0)
					return true;
			}

			return false;
		}
	}
}
