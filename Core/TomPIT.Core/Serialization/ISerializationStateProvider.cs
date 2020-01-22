namespace TomPIT.Serialization
{
	public interface ISerializationStateProvider
	{
		object SerializationState { get; set; }
	}
}
