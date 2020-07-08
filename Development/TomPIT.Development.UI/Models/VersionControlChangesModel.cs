using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Exceptions;
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
			return Tenant.GetService<IVersionControlService>().GetDiff(Arguments.Required<Guid>("component"), Arguments.Required<Guid>("id"));
		}

		public void Commit()
		{
			var components = Arguments.Required<JArray>("components");
			var items = new List<Guid>();

			foreach (JObject component in components)
				items.Add(component.Required<Guid>("component"));

			if (items.Count == 0)
				throw new RuntimeException(SR.ErrCommitNoFiles);

			Tenant.GetService<IVersionControlService>().Commit(items, Arguments.Required<string>("comment"));
		}

		public void Undo()
		{
			var components = Arguments.Required<JArray>("components");
			var items = new List<Guid>();

			foreach (JObject component in components)
				items.Add(component.Required<Guid>("component"));

			Tenant.GetService<IVersionControlService>().Undo(items);
		}

		public IChangeDescriptor GetChanges()
		{
			return Tenant.GetService<IVersionControlService>().GetChanges(ChangeQueryMode.MetaData);
		}

		public List<IRepositoryBinding> QueryActiveBindings()
		{
			return Tenant.GetService<IVersionControlService>().QueryActiveBindings();
		}
	}
}
