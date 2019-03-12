namespace TomPIT.Cdn.Dns
{
	internal class ExceptionRecord : TextOnly
	{
		public ExceptionRecord(string msg)
		{
			Strings.Add(msg);
		}
	}
}
