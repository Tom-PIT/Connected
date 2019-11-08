using TomPIT.Data.DataProviders.Design;

namespace TomPIT.DataProviders.Sql.Design
{
	internal class GroupObject : IGroupObject
	{
		public string Text { get; set; }

		public string Value { get; set; }

		public string Description { get; set; }
	}
}
