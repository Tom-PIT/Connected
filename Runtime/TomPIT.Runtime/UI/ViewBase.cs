using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Net;
using TomPIT.Models;
using TomPIT.Serialization;
using AA = TomPIT.Annotations.Design.AnalyzerAttribute;
using CIP = TomPIT.Annotations.Design.CompletionItemProviderAttribute;

namespace TomPIT.Runtime.UI
{
	public abstract class ViewBase<T> : RazorPage<T>
	{
		protected Guid ComponentId { get; set; }
		protected string ViewType { get; set; }

		private IViewModel ViewModel => Model as IViewModel;

		protected string GetString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key)
		{
			return ViewModel.Services.Globalization.GetString(stringTable, key);
		}

		protected HtmlString GetHtmlString([CIP(CIP.StringTableProvider)][AA(AA.StringTableAnalyzer)] string stringTable, [CIP(CIP.StringTableStringProvider)][AA(AA.StringAnalyzer)] string key)
		{
			return new HtmlString(ViewModel.Services.Globalization.GetString(stringTable, key));
		}

		protected string Serialize(object content)
		{
			if (content == null)
				return "null";

			return Serializer.Serialize(content);
		}
		[Obsolete("Please use Serialize() instead")]
		protected string ToJsonString(object content)
		{
			return Serialize(content);
		}

		protected void NotFound()
		{
			Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
		}

		protected void BadRequest()
		{
			Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
		}
		protected void Deny()
		{
			if (ViewModel.Services.Identity.User == null)
				Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
			else
				Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
		}

	}
}