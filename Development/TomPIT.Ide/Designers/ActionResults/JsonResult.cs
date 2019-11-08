namespace TomPIT.Ide.Designers.ActionResults
{
	public class JsonResult : Result, IDesignerActionResultJson
	{
		public JsonResult(object model, object data) : base(model)
		{
			Data = data;
		}

		public object Data { get; private set; }
	}
}
