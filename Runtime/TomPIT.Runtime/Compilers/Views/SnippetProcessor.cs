using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Runtime.Compilers.Views
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

		public override void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
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
