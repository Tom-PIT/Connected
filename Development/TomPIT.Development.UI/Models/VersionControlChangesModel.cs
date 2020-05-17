using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Ide.VersionControl;

namespace TomPIT.Development.Models
{
	public class VersionControlChangesModel : DevelopmentModel
	{
		public VersionControlChangesModel(JObject arguments)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }

		public IVersionControlDiffDescriptor GetDiff()
		{
			return Tenant.GetService<IVersionControlService>().GetDiff(Arguments.Required<Guid>("component"), Arguments.Required<Guid>("blob"));
		}

		public List<IVersionControlDescriptor> GetChanges()
		{
			return Tenant.GetService<IVersionControlService>().GetChanges();
		}
	}
}
