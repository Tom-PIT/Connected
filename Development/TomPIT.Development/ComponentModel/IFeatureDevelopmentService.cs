using System;

namespace TomPIT.ComponentModel
{
	public interface IFeatureDevelopmentService
	{
		Guid Insert(Guid microService, string name);
		void Update(Guid microService, Guid feature, string name);
		void Delete(Guid microService, Guid feature);
	}
}
