using TomPIT.Middleware;

namespace TomPIT.BigData
{
	/// <summary>
	/// Defines how timestamp column behaves.
	/// </summary>
	public enum TimestampBehavior
	{
		/// <summary>
		/// Once a record is created it's timestamp value is static. It means it's not
		/// included in the update statement
		/// </summary>
		Static = 1,
		/// <summary>
		/// Record is always updated with new timestamp value.
		/// </summary>
		Dynamic = 2
	}
	public interface IPartitionComponent : IMiddlewareComponent
	{
		TimestampBehavior Timestamp { get; }
	}
}
