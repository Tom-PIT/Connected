using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Annotations.Design;
using TomPIT.Design;
using TomPIT.Exceptions;
using TomPIT.Ide;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Environment.Providers;
using TomPIT.MicroServices.IoT.UI;
using TomPIT.MicroServices.IoT.UI.Stencils;
using TomPIT.MicroServices.IoT.UI.Stencils.Shapes;
using TomPIT.Reflection;

namespace TomPIT.MicroServices.IoT.Design.Designers
{
	public class IoTViewDesigner : DomDesigner<IDomElement>, IDesignerSelectionProvider
	{
		private object _value = null;
		private string _selectionId = string.Empty;

		public IoTViewDesigner(IDomElement element) : base(element)
		{
			Toolbox.Items.Add(new ItemDescriptor("Rectangle", "Rectangle", typeof(Rectangle)) { Category = "Shapes", Glyph = "fal fa-rectangle-landscape" });
			Toolbox.Items.Add(new ItemDescriptor("Text", "Text", typeof(Text)) { Category = "Shapes", Glyph = "fal fa-rectangle-landscape" });
		}

		public override string View => "~/Views/Ide/Designers/IoTDesigner.cshtml";
		public override object ViewModel => this;

		public IoTView IoTView => Element.Component as IoTView;

		public object Value
		{
			get
			{
				if (_value == null)
				{
					_value = IoTView;

					if (!string.IsNullOrWhiteSpace(SelectionId))
					{
						var id = new Guid(SelectionId);

						if (id == Guid.Empty)
							return _value;

						var target = Environment.Context.Tenant.GetService<IDiscoveryService>().Find(IoTView, id);

						if (target != null)
						{
							_value = target;
							Environment.Selection.Reset();
						}
					}
				}

				return _value;
			}
		}

		public string SelectionId { get { return Environment.RequestBody.Optional("designerSelectionId", string.Empty); } }

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "add", true) == 0)
				return Add(data);
			else if (string.Compare(action, "saveProperties", true) == 0)
				return SaveProperties(data);
			else if (string.Compare(action, "remove", true) == 0)
				return Remove(data);
			else if (string.Compare(action, "viewInfo", true) == 0)
				return ViewInfo(data);

			return base.OnAction(data, action);
		}

		private IDesignerActionResult ViewInfo(JObject data)
		{
			return Result.JsonResult(ViewModel, new JObject
			{
				{ "width", IoTView.Width },
				{ "height", IoTView.Height },
			});
		}

		private IDesignerActionResult Remove(JObject data)
		{
			var items = data.Optional<JArray>("items", null);

			foreach (JValue i in items)
			{
				var id = Types.Convert<Guid>(i.Value);

				var element = IoTView.Elements.FirstOrDefault(f => f.Id == id);

				if (element != null)
					IoTView.Elements.Remove(element);
			}

			ResizeView();
			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(IoTView);

			return Result.SectionResult(ViewModel, EnvironmentSection.Properties);
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

			ResizeView();
			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(IoTView);

			return Result.SectionResult(ViewModel, EnvironmentSection.Properties);
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
			stencil.Name = Environment.Context.Tenant.GetService<INamingService>().Create(ti.Text, IoTView.Elements.Select(f => f.Name));

			IoTView.Elements.Add(stencil);
			ResizeView();

			var att = stencil.GetType().FindAttribute<ComponentCreatingHandlerAttribute>();

			if (att != null)
			{
				var handler = att.Type == null
					? TypeExtensions.GetType(att.TypeName).CreateInstance<IComponentCreateHandler>()
					: att.Type.CreateInstance<IComponentCreateHandler>();

				if (handler != null)
					handler.InitializeNewComponent(Environment.Context, stencil);
			}

			Environment.Context.Tenant.GetService<IComponentDevelopmentService>().Update(IoTView);

			return Result.ViewResult(stencil.CreateModel(Environment.Context), "~/Views/Ide/Designers/IoT/Stencil.cshtml");
		}

		private void ResizeView()
		{
			var width = 0;
			var height = 0;

			foreach (var i in IoTView.Elements)
			{
				if (i.Right() > width)
					width = i.Right();

				if (i.Bottom() > height)
					height = i.Bottom();
			}

			IoTView.Width = width;
			IoTView.Height = height;
		}
	}
}
