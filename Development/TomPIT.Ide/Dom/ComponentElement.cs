using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Dom
{
	public class ComponentElement : ReflectionElement
	{
		private IConfiguration _configuration = null;
		private object[] _propertySources = null;
		private IDomDesigner _designer = null;

		public ComponentElement(IDomElement parent, IComponent component) : base(parent, component)
		{
			Id = Target.Token.ToString();
			Glyph = "fal fa-file";
			Title = Target.Name;

			((Behavior)Behavior).AutoExpand = false;
			((Behavior)Behavior).Static = false;

			Verbs.Add(new Verb
			{
				Action = VerbAction.Ide,
				Confirm = string.Format("Are you sure you want to delete component '{0}'?", Title),
				Id = "deleteComponent",
				Name = "Delete component"
			});
		}

		protected IComponent Target { get { return Instance as IComponent; } }

		private IConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = Connection.GetService<IComponentService>().SelectConfiguration(Target.Token);

				return _configuration;
			}
		}

		public override object Component => Configuration;

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
					var att = Configuration.GetType().ResolveDesigner();

					if (att != null)
						_designer = DomQuery.CreateDesigner(Environment, this, att);

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
