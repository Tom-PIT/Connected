using System.Collections.Generic;
using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	internal class Index : ITableIndex
	{
		private List<string> _columns = null;
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
		[JsonProperty(PropertyName = "columns")]
		public List<string> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<string>();

				return _columns;
			}
		}
	}
}
