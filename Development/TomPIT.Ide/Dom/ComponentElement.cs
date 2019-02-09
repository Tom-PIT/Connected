using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Services;

namespace TomPIT.Dom
{
	public class ComponentElement : ReflectionElement
	{
		private IConfiguration _configuration = null;
		private object[] _propertySources = null;
		private IDomDesigner _designer = null;
		private bool _loaded = false;

		public ComponentElement(IDomElement parent, IComponent component) : base(parent, component)
		{
			Id = Target.Token.ToString();
			Glyph = ResolveGlyph();

			if (_loaded && Configuration == null)
				Title = string.Format("(!){0}", Target.Name);
			else
				Title = Target.Name;

			((Behavior)Behavior).AutoExpand = false;
			((Behavior)Behavior).Static = false;

			var md = MetaData as ElementMetaData;

			md.Category = Target.Category;

			Verbs.Add(new Verb
			{
				Action = VerbAction.Ide,
				Confirm = string.Format("Are you sure you want to delete component '{0}'?", Title),
				Id = "deleteComponent",
				Name = "Delete component"
			});
		}

		private string ResolveGlyph()
		{
			var r = "fal fa-file";
			var id = Target.Category;
			var template = GetService<IMicroServiceTemplateService>().Select(DomQuery.Closest<IMicroServiceScope>(this).MicroService.Template);

			var items = template.ProvideAddItems(null);
			var item = items.FirstOrDefault(f => string.Compare(Target.Type, f.Type.TypeName(), true) == 0);

			if (item != null && !string.IsNullOrWhiteSpace(item.Glyph))
				r = item.Glyph;

			return r;
		}

		protected IComponent Target { get { return Instance as IComponent; } }

		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null && !_loaded)
				{
					_loaded = true;

					try
					{
						_configuration = Connection.GetService<IComponentService>().SelectConfiguration(Target.Token);
					}
					catch (RuntimeException ex)
					{
						if (ex.EventId != ExecutionEvents.Deserialize)
							throw ex;
					}
				}

				return _configuration;
			}
		}

		public override object Component
		{
			get
			{
				if (Configuration != null)
					return Configuration;

				return Target;
			}
		}

		public override object[] PropertySources
		{
			get
			{
				if (_propertySources == null)
					_propertySources = new object[] { Target, Configuration };

				return _propertySources;
			}
		}

		public override bool Commit(object component, string property, string attribute)
		{
			if (component == Target)
				Connection.GetService<IComponentDevelopmentService>().Update(Target.Token, Target.Name, Target.Folder);
			else
				Connection.GetService<IComponentDevelopmentService>().Update(Configuration);

			return true;
		}

		public override IDomDesigner Designer
		{
			get
			{
				if (_designer == null)
				{
					if (Configuration == null)
						return null;

					var att = Configuration.GetType().ResolveDesigner();

					if (att != null)
						_designer = DomQuery.CreateDesigner(this, att);

					if (_designer == null && IsDesignTime)
					{
						var de = Value.GetType().FindAttribute<System.ComponentModel.DefaultEventAttribute>();

						if (de != null)
						{
							_designer = PropertyDesigner(de.Name);

							if (_designer != null)
								Environment.RequestBody["property"] = de.Name;
						}
					}
				}

				return _designer;
			}
		}
	}
}
