using TomPIT.Annotations;
using TomPIT.Ide;

namespace TomPIT.ActionResults
{
	public abstract class Result : IDesignerActionResult
	{
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
	}
}
