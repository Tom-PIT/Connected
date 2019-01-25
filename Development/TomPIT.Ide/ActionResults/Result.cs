using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TomPIT.Annotations;

namespace TomPIT.ActionResults
{
	public abstract class Result : IDesignerActionResult
	{
		private Dictionary<string, string> _responseHeaders = null;

		protected Result(object model)
		{
			Model = model;
		}

		public object Model { get; private set; }

		public InformationKind MessageKind { get; set; } = InformationKind.None;

		public string Message { get; set; }

		public string Title { get; set; }

		public string ExplorerPath { get; set; }

		public static SectionResult SectionResult(object model, EnvironmentSection sections)
		{
			return new SectionResult(model, sections);
		}

		public static SectionResult SectionResult(object model, EnvironmentSection sections, JObject data)
		{
			return new SectionResult(model, sections, data);
		}

		public static EmptyResult EmptyResult(object model)
		{
			return new EmptyResult(model);
		}
		public static JsonResult JsonResult(object model, object data)
		{
			return new JsonResult(model, data);
		}

		public static ViewResult ViewResult(object model, string view)
		{
			return new ViewResult(model, view);
		}

		public Dictionary<string, string> ResponseHeaders
		{
			get
			{
				if (_responseHeaders == null)
					_responseHeaders = new Dictionary<string, string>();

				return _responseHeaders;
			}
		}
	}
}
