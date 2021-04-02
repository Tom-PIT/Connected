namespace TomPIT.Reflection
{
	public interface IManifestDistributedType: IManifestType
	{
		bool IsDistributed { get; }
	}
}
