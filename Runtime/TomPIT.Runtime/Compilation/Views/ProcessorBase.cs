using System;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;

namespace TomPIT.Compilation.Views
{
	internal abstract class ProcessorBase
	{
		public ProcessorBase(string source)
		{
			Source = source;
		}

		protected string Source { get; }

		public virtual void Compile(ITenant tenant, IComponent component, IConfiguration configuration)
		{

		}

		public abstract string Result { get; }

		protected void AddUsings(StringBuilder builder)
		{
			builder.AppendLine("@using TomPIT;");
		}

		protected void AppendViewMetaData(StringBuilder builder, string viewType, Guid componentId)
		{
			builder.AppendLine("@{");
			builder.AppendLine(string.Format("ViewType = \"{0}\";", viewType));
			builder.AppendLine(string.Format("ComponentId = new Guid(\"{0}\");", componentId));
			builder.AppendLine("}");
		}

		protected void AppendBaseType(StringBuilder builder, string baseType)
		{
			builder.AppendLine($"@inherits {baseType}<TModel>");
		}

		protected void AppendBaseType(StringBuilder builder)
		{
			builder.AppendLine($"@inherits TomPIT.Runtime.UI.ViewBase<TomPIT.Models.IViewModel>");
		}

		protected void AddTagHelpers(StringBuilder builder)
		{
			builder.AppendLine("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers");
			builder.AppendLine("@addTagHelper *, TomPIT.Extensions");
		}

		protected string SelectScripts(ITenant tenant, Guid microService, IGraphicInterface config)
		{
			var r = new StringBuilder();

			foreach (var script in config.Scripts)
			{
				var scr = tenant.GetService<IComponentService>().SelectText(microService, script);

				if (!string.IsNullOrWhiteSpace(scr))
				{
					r.Append(scr);
					r.AppendLine();
				}
			}

			return r.ToString();
		}
	}
}
