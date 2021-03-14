namespace TomPIT.Security
{
	public enum ElevationContextState
	{
		NotSet = 0,
		Granted = 1,
		Revoked = 2,
		Pending = 3
	}
	public interface IElevationContext
	{
		ElevationContextState State { get; set; }
		object AuthorizationOwner { get; set; }
	}
}
