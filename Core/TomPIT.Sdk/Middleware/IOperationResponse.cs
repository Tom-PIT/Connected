namespace TomPIT.Middleware
{
	public enum ResponseResult
	{
		Agree = 1,
		Objection = 2
	}

	public interface IOperationResponse
	{
		ResponseResult Result { get; }
		string Message { get; }
		string Handler { get; }
	}
}
