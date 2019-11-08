using TomPIT.ComponentModel.IoC;

namespace TomPIT.IoC
{
	public interface IIoCOperationContext
	{
		IIoCOperation Operation { get; set; }
	}
}
