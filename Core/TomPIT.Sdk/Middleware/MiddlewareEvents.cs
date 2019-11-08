namespace TomPIT.Middleware
{
	public class MiddlewareEvents
	{
		public const int Runtime = 100;
		public const int CreateConnection = 101;
		public const int DataRead = 102;
		public const int DataWrite = 103;
		public const int OpenConnection = 104;
		public const int PrepareCommand = 105;
		public const int Deserialize = 106;
		/*
		 * CDN
		 */
		public const int SendMail = 601;
	}
}
