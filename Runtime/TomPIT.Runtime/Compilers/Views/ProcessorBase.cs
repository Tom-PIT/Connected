using System;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Services;

namespace TomPIT.Runtime.Compilers.Views
{
	internal abstract class ProcessorBase
	{
		public ProcessorBase(string source)
		{
			Source = source;
		}

		protected string Source { get; }

		public virtual void Compile(ISysConnection connection, IComponent component, IConfiguration configuration)
		{

		}

		public abstract string Result { get; }

		protected void AddUsings(StringBuilder builder)
		{
			builder.AppendLine("@using TomPIT;");
			builder.AppendLine("@using System;");
			builder.AppendLine("@using System.Linq;");
			builder.AppendLine("@using System.Text;");
			builder.AppendLine("@using System.Data;");
			builder.AppendLine("@using Newtonsoft.Json;");
			builder.AppendLine("@using Newtonsoft.Json.Linq;");
			builder.AppendLine("@using System.Collections;");
			builder.AppendLine("@using System.Collections.Generic;");
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
			AppendBaseType(builder, "TomPIT.UI.ViewBase");
		}

		protected void AddTagHelpers(StringBuilder builder)
		{
			builder.AppendLine("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers");
			builder.AppendLine("@addTagHelper *, TomPIT.Extensions");
		}

		protected string SelectScripts(ISysConnection connection, Guid microService, IGraphicInterface config)
		{
			var r = new StringBuilder();

			foreach (var script in config.Scripts)
			{
				var scr = connection.GetService<IComponentService>().SelectText(microService, script);

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
