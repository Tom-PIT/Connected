namespace TomPIT.Middleware
{
	public class OperationResponse : IOperationResponse
	{
		public ResponseResult Result { get; set; } = ResponseResult.Objection;

		public string Message { get; set; }

		public string Handler { get; set; }
	}
}
