using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.ActionResults;
using TomPIT.ComponentModel;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide.Design.VersionControl;
using TomPIT.Security;

namespace TomPIT.Development.Designers
{
	public class VersionControlChangesDesigner : DomDesigner<Dom.Element>
	{
		private List<IComponent> _changes = null;

		public VersionControlChangesDesigner(Dom.Element element) : base(element)
		{
		}

		private IMicroService MicroService => Element.Closest<IMicroServiceScope>().MicroService;
		public override object ViewModel => this;
		public override string View => "~/Views/Ide/Designers/VersionControlChanges.cshtml";

		public List<IComponent> Changes
		{
			get
			{
				if (_changes == null)
					_changes = Connection.GetService<IVersionControlService>().Changes(MicroService.Token);

				return _changes;
			}
		}

		public string Glyph(IComponent component)
		{
			return component.Glyph(Environment.Context.Connection());
		}

		public IUser GetUser(Guid user)
		{
			return GetService<IUserService>().Select(user.ToString());
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

			Connection.GetService<IVersionControlService>().Commit(r, comment);

			return Result.SectionResult(this, Annotations.EnvironmentSection.Designer);
		}

		private IDesignerActionResult Undo(JObject data)
		{
			var r = new List<Guid>();
			var a = data.Required<JArray>("items");

			foreach (JValue i in a)
				r.Add(Types.Convert<Guid>(i.Value));

			Connection.GetService<IVersionControlService>().Undo(r);

			return Result.SectionResult(this, Annotations.EnvironmentSection.Designer);
		}
	}
}
