using TomPIT.ComponentModel.Data;

namespace TomPIT.Data
{
	public interface IModelService
	{
		void SynchronizeEntity(IModelConfiguration configuration);
	}
}
