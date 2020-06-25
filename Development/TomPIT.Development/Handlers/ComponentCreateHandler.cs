using System.IO;
using System.Text.RegularExpressions;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Development.Handlers
{
	public abstract class ComponentCreateHandler<T> : IComponentCreateHandler where T : class, IText
	{
		private Regex _rx = null;
		protected T Instance { get; private set; }
		protected IMiddlewareContext Context { get; private set; }
		protected string ComponentName { get; private set; }

		public void InitializeNewComponent(IMiddlewareContext context, object instance)
		{
			Instance = instance as T;
			Context = context;

			if (Instance == null)
				return;

			ComponentName = Instance.Configuration().ComponentName();

			OnInitializeNewComponent();
		}

		protected virtual void OnInitializeNewComponent()
		{
			using var stream = GetType().Assembly.GetManifestResourceStream(Template);
			using var reader = new StreamReader(stream);
			var text = Regex.Replace(reader.ReadToEnd(), OnReplace);

			Context.Tenant.GetService<IComponentDevelopmentService>().Update(Instance as IText, text);
		}

		protected abstract string Template { get; }

		protected Regex Regex
		{
			get
			{
				if (_rx == null)
					_rx = new Regex("\\[[\\s\\S][^\\[]*?\\]");

				return _rx;
			}
		}

		protected virtual string OnReplace(Match match)
		{
			if (string.Compare(match.Value, "[NAME]", false) == 0)
				return ComponentName;

			return match.Value;
		}

		protected string Singular(string value)
		{
			if (value.EndsWith("s", System.StringComparison.OrdinalIgnoreCase))
				return value[0..^1];

			return value;
		}
	}
}
