using System;
using System.Text;

namespace TomPIT.UI
{
	internal abstract class ProcessorBase
	{
		public ProcessorBase(string source)
		{
			Source = source;
		}

		protected string Source { get; }

		public virtual void Compile()
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

		protected void AppendBaseType(StringBuilder builder)
		{
			builder.AppendLine("@inherits TomPIT.UI.ViewBase<TModel>");
		}

		protected void AddTagHelpers(StringBuilder builder)
		{
			builder.AppendLine("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers");
			builder.AppendLine("@addTagHelper *, TomPIT.Extensions");
		}
	}
}
