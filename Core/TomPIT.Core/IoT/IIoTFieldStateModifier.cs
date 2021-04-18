namespace TomPIT.IoT
{
	public interface IIoTFieldStateModifier
	{
		string Field { get; }
		string Value { get; }
		string Device { get; }
		object RawValue { get; }
	}
}
