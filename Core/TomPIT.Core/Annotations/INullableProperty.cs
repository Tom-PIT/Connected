namespace TomPIT.Annotations
{
	public interface INullableProperty
	{
		object MappedValue { get; }
		bool IsNull { get; }
	}
}
