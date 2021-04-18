using Newtonsoft.Json.Linq;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Toolbox;
using TomPIT.Ide.Designers.ActionResults;
using TomPIT.Ide.Environment;
using TomPIT.Ide.Resources;

namespace TomPIT.Ide.Designers
{
	public abstract class DomDesigner<E> : EnvironmentObject, IDomDesigner, IDomObject where E : IDomElement
	{
		private IDesignerToolbar _toolbar = null;
		private IToolbox _toolbox = null;

		protected DomDesigner(E element) : base(element.Environment)
		{
			Owner = element;
		}

		protected object Component { get { return Owner?.Value; } }
		public E Owner { get; private set; }
		public IDomElement Element { get { return Owner; } }

		public virtual IDesignerToolbar Toolbar
		{
			get
			{
				if (_toolbar == null)
				{
					_toolbar = new DesignerToolbar(Environment);

					OnCreateToolbar(_toolbar);
				}

				return _toolbar;
			}
		}

		protected virtual void OnCreateToolbar(IDesignerToolbar toolbar)
		{

		}

		public IDesignerActionResult Action(JObject data)
		{
			var action = data.Optional("action", string.Empty);

			if (string.IsNullOrWhiteSpace(action))
				throw IdeException.ExpectedParameter(this, 0, "action");

			return OnAction(data, action);
		}

		protected virtual IDesignerActionResult OnAction(JObject data, string action)
		{
			return Result.EmptyResult(this);
		}

		protected JObject Request { get { return Environment.RequestBody; } }

		public virtual string View { get { return null; } }
		public virtual string PropertyView { get { return null; } }
		public virtual object ViewModel { get { return null; } }

		public virtual string Path { get { return DomQuery.Path(Element); } }

		public virtual bool IsPropertyEditable(string propertyName)
		{
			return true;
		}

		public virtual bool SupportsChaining { get; } = true;

		public IToolbox Toolbox
		{
			get
			{
				if (_toolbox == null)
					_toolbox = new Toolbox(Environment);

				return _toolbox;
			}
		}
	}
}
