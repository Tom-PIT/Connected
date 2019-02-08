using Newtonsoft.Json.Linq;
using System.Linq;
using TomPIT.ActionResults;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Designers
{
	public class ListDesigner<E> : CollectionDesigner<E>, ISupportsAddDesigner where E : IDomElement
	{
		private object _proposed = null;
		private IPropertyProvider _properties = null;

		public ListDesigner(E element) : base(element)
		{
		}

		public override string View { get { return "~/Views/Ide/Designers/List.cshtml"; } }

		public override string PropertyView
		{
			get
			{
				var mode = Environment.RequestBody.Optional("mode", string.Empty);

				if (string.Compare(mode, "add", true) == 0)
					return "~/Views/Ide/Designers/ListAdd.cshtml";

				return null;
			}
		}

		public string DescriptorId { get { return Environment.RequestBody.Optional("id", string.Empty); } }
		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "saveCreate", true) == 0)
			{
				var values = data.Optional<JArray>("values", null);
				var writer = new PropertyWriter(Element);

				if (values != null)
				{
					foreach (JObject i in values)
					{
						var p = i.Required<string>("property");
						var v = i.Optional("value", string.Empty);

						writer.Write(ProposedComponent, p, v);
					}

					return OnCreateComponent(ProposedComponent);
				}
			}

			return base.OnAction(data, action);
		}

		protected virtual IDesignerActionResult OnCreateComponent(object component)
		{
			return Result.EmptyResult(this);
		}

		public virtual object ProposedComponent
		{
			get
			{
				if (_proposed == null)
				{
					if (string.IsNullOrWhiteSpace(DescriptorId))
						return null;

					var d = Descriptors.FirstOrDefault(f => string.Compare(DescriptorId, f.Id, true) == 0);

					if (d == null)
						return null;

					_proposed = CreateProposedInstance(d);
				}

				return _proposed;
			}
		}

		protected virtual object CreateProposedInstance(IItemDescriptor d)
		{
			if (d.Type == null)
				return null;

			var df = d.Type.CreateInstance();

			if (df == null)
				throw IdeException.CannotCreateInstance(this, IdeEvents.DesignerAction, d.Type);

			IdeExtensions.ProcessComponentCreating(Environment.Context, df);

			return df;
		}

		public IPropertyProvider Properties
		{
			get
			{
				if (_properties == null)
					_properties = new PropertyProvider(Element, ProposedComponent);

				return _properties;
			}
		}
	}
}