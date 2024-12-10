using TomPIT.ComponentModel;

namespace TomPIT.Compilation;
internal sealed class CompilationDescriptor
{
	public required IMicroService MicroService { get; init; }
	public bool ShouldRecompile { get; init; }
	public bool IsSupported { get; init; }
}
