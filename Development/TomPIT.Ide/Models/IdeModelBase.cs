using Newtonsoft.Json.Linq;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Runtime;
using TomPIT.Services;

namespace TomPIT.Models
{
	public abstract class IdeModelBase : ShellModel, IEnvironment
	{
		private IDom _dom = null;
		private IGlobalization _globalization = null;
		private ISelection _selection = null;

		public virtual IDom Dom
		{
			get
			{
				if (_dom == null)
					_dom = CreateDom();

				return _dom;
			}
		}

		protected abstract IDom CreateDom();

		public virtual IGlobalization Globalization
		{
			get
			{
				if (_globalization == null)
					_globalization = new Ide.Globalization(this);

				return _globalization;
			}
		}

		public virtual ISelection Selection
		{
			get
			{
				if (_selection == null)
					_selection = new Selection(this);

				return _selection;
			}
		}

		public IExecutionContext Context => this;

		public abstract string Id { get; }
		public JObject RequestBody { get; set; }
		public string Path { get; set; }

		public abstract string IdeUrl { get; }
	}
}