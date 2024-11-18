namespace TomPIT.ComponentModel.Resources;

public interface IStaticEmbeddedResource : IUploadResource
{
	string? Url { get; }
}