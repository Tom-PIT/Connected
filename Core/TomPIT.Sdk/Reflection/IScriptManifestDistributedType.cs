namespace TomPIT.Reflection
{
	public interface IScriptManifestDistributedType: IScriptManifestType
	{
		bool IsDistributed { get; }
	}
}
