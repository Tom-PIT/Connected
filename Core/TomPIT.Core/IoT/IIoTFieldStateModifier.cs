namespace TomPIT.IoT
{
	public interface IIoTFieldStateModifier
	{
		string Field { get; set; }
		string Value { get; set; }
		string Device { get; set; }
		object RawValue { get; }
	}
}
