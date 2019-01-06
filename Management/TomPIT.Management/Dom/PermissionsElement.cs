using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	public class PermissionsElement : Element
	{
		public const string FolderId = "Permissions";
		private IMicroServiceTemplate _template = null;
		private List<IDomElement> _root = null;
		private IDomDesigner _designer = null;

		public PermissionsElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = FolderId;
			Glyph = "fal fa-shield-check";
			Title = SR.DomPermissions;
		}

		public override bool HasChildren { get { return Root != null && Root.Count > 0; } }

		public override void LoadChildren()
		{
			if (Root == null)
				return;

			foreach (var i in Root)
				Items.Add(i);
		}

		public override void LoadChildren(string id)
		{
			if (Root == null)
				return;

			var d = Root.FirstOrDefault(f => f.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

			if (d != null)
				Items.Add(d);
		}

		private IMicroService MicroService { get { return DomQuery.Closest<IMicroServiceScope>(this).MicroService; } }
		private IMicroServiceTemplate Template
		{
			get
			{
				if (_template == null)
					_template = SysContext.GetService<IMicroServiceTemplateService>().Select(MicroService.Template);

				return _template;
			}
		}

		private List<IDomElement> Root
		{
			get
			{
				if (_root == null && Template != null)
					_root = Template.QuerySecurityRoot(this);

				return _root;
			}
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
					_designer = new EmptyDesigner(Environment, this);

				return _designer;
			}
		}
	}
}
