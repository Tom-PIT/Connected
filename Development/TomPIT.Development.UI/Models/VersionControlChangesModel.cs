using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Design;
using TomPIT.Exceptions;

namespace TomPIT.Development.Models
{
	public class VersionControlChangesModel : DevelopmentModel
	{
		public VersionControlChangesModel(JObject arguments)
		{
			Arguments = arguments;
		}

		public JObject Arguments { get; }

		public IDiffDescriptor GetDiff()
		{
			return Tenant.GetService<IDesignService>().VersionControl.GetDiff(Arguments.Required<Guid>("component"), Arguments.Required<Guid>("id"));
		}

		public void Commit()
		{
			var components = Arguments.Required<JArray>("components");
			var items = new List<Guid>();

			foreach (JObject component in components)
				items.Add(component.Required<Guid>("component"));

			if (items.Count == 0)
				throw new RuntimeException(SR.ErrCommitNoFiles);

			Tenant.GetService<IDesignService>().VersionControl.Commit(items, Arguments.Required<string>("comment"));
		}

		public void Undo()
		{
			var components = Arguments.Required<JArray>("components");
			var items = new List<Guid>();

			foreach (JObject component in components)
				items.Add(component.Required<Guid>("component"));

			Tenant.GetService<IDesignService>().VersionControl.Undo(items);
		}

		public IChangeDescriptor GetChanges()
		{
			return Tenant.GetService<IDesignService>().VersionControl.GetChanges(ChangeQueryMode.MetaData);
		}

		public List<IServiceBinding> QueryActiveBindings()
		{
			return Tenant.GetService<IDesignService>().Bindings.QueryActive();
		}
	}
}
