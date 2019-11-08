using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Ide;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Ide.VersionControl;
using TomPIT.Security;

namespace TomPIT.Development.Designers
{
	public class VersionControlChangesDesigner : DomDesigner<DomElement>
	{
		private List<IComponent> _changes = null;

		public VersionControlChangesDesigner(DomElement element) : base(element)
		{
		}

		public IMicroService MicroService => Element.Closest<IMicroServiceScope>().MicroService;
		public override object ViewModel => this;
		public override string View => "~/Views/Ide/Designers/VersionControlChanges.cshtml";

		public List<IComponent> Changes
		{
			get
			{
				if (_changes == null)
					_changes = Environment.Context.Tenant.GetService<IVersionControlService>().Changes();

				return _changes;
			}
		}

		public string Glyph(IComponent component)
		{
			return component.Glyph(Environment.Context.Tenant);
		}

		public IUser GetUser(Guid user)
		{
			return Environment.Context.Tenant.GetService<IUserService>().Select(user.ToString());
		}

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "undo", true) == 0)
				return Undo(data);
			else if (string.Compare(action, "commit", true) == 0)
				return Commit(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Commit(JObject data)
		{
			var r = new List<Guid>();
			var comment = data.Required<string>("comment");
			var a = data.Required<JArray>("items");

			foreach (JValue i in a)
				r.Add(Types.Convert<Guid>(i.Value));

			Environment.Context.Tenant.GetService<IVersionControlService>().Commit(r, comment);

			return Result.SectionResult(this, EnvironmentSection.Designer);
		}

		private IDesignerActionResult Undo(JObject data)
		{
			var r = new List<Guid>();
			var a = data.Required<JArray>("items");

			foreach (JValue i in a)
				r.Add(Types.Convert<Guid>(i.Value));

			Environment.Context.Tenant.GetService<IVersionControlService>().Undo(r);

			return Result.SectionResult(this, EnvironmentSection.Designer);
		}

		public string MicroServiceName(Guid microService)
		{
			var ms = Environment.Context.Tenant.GetService<IMicroServiceService>().Select(microService);

			return ms == null ? "?" : ms.Name;
		}
	}
}
