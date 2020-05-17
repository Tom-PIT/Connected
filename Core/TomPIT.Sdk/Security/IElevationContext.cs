namespace TomPIT.Security
{
	public enum ElevationContextState
	{
		Granted = 1,
		Revoked = 2
	}
	public interface IElevationContext
	{
		ElevationContextState State { get; }
		void Grant();
		void Revoke();
	}
}
