using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;

namespace TomPIT.UI
{
	internal class SnippetProcessor : ProcessorBase
	{
		private StringBuilder _sb = null;
		private IComponent _component = null;

		public SnippetProcessor(ISnippet snippet, string source) : base(source)
		{
			Snippet = snippet;
		}

		private ISnippet Snippet { get; }
		private IComponent Component
		{
			get
			{
				if (_component == null)
					_component = Instance.GetService<IComponentService>().SelectComponent(Snippet.Configuration().Component);

				return _component;
			}
		}

		public override void Compile()
		{
			AddUsings(Builder);
			Builder.AppendLine(string.Format("@model {0}", ResolveModel()));

			Builder.Append(Source);
		}

		private string ResolveModel()
		{
			return "TomPIT.Models.IViewModel";
		}

		public override string Result { get { return Builder.ToString(); } }

		private StringBuilder Builder
		{
			get
			{
				if (_sb == null)
					_sb = new StringBuilder();

				return _sb;
			}
		}
	}
}
