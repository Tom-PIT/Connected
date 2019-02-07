using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.IoT.UI;
using TomPIT.IoT.UI.Stencils;
using TomPIT.IoT.UI.Stencils.Shapes;

namespace TomPIT.IoT.Designers
{
	public class IoTViewDesigner : DomDesigner<IDomElement>
	{
		public IoTViewDesigner(IDomElement element) : base(element)
		{
			Toolbox.Items.Add(new ItemDescriptor("Rectangle", "Rectangle", typeof(Rectangle)) { Category = "Shapes", Glyph = "fal fa-rectangle-landscape" });
		}

		public override string View => "~/Views/Ide/Designers/IoTView.cshtml";
		public override object ViewModel => this;

		public IoTView IoTView => Element.Component as IoTView;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "add", true) == 0)
				return Add(data);
			else if (string.Compare(action, "saveProperties", true) == 0)
				return SaveProperties(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult SaveProperties(JObject data)
		{
			var elements = data.Optional<JArray>("elements", null);

			if (elements == null || elements.Count == 0)
				return Result.EmptyResult(ViewModel);

			foreach (JObject i in elements)
			{
				var id = i.Required<Guid>("id");

				var element = IoTView.Elements.FirstOrDefault(f => f.Id == id);

				if (element == null)
					throw new RuntimeException("Element not found.");

				foreach (var j in i)
				{
					if (string.Compare(j.Key, "id", true) == 0)
						continue;

					SaveProperty(element, j.Key, j.Value as JValue);
				}
			}

			GetService<IComponentDevelopmentService>().Update(IoTView);

			return Result.SectionResult(ViewModel, TomPIT.Annotations.EnvironmentSection.Properties);
		}

		private void SaveProperty(IIoTElement element, string key, JValue value)
		{
			var property = DomQuery.ResolveProperty(element, key);

			property.Item1.SetValue(property.Item2, Types.Convert(value.Value, property.Item1.PropertyType));
		}

		private IDesignerActionResult Add(JObject data)
		{
			var id = data.Required<string>("id");
			var x = data.Required<int>("x");
			var y = data.Required<int>("y");

			var ti = Toolbox.Items.FirstOrDefault(f => string.Compare(f.Id, id, true) == 0);
			var stencil = ti.Type.CreateInstance<IoTElement>();

			stencil.Left = x;
			stencil.Top = y;
			stencil.Name = GetService<INamingService>().Create(ti.Text, IoTView.Elements.Select(f => f.Name));

			IoTView.Elements.Add(stencil);

			GetService<IComponentDevelopmentService>().Update(IoTView);

			return Result.ViewResult(stencil.CreateModel(Environment.Context), stencil.View());
		}
	}
}
