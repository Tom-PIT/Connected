using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Development;
using TomPIT.Sys.Api.Database;

namespace TomPIT.Sys.Data
{
	internal class DevelopmentErrors
	{
		public List<IDevelopmentError> Query(Guid microService)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			return Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Query(ms);
		}

		public void Clear(Guid component, Guid element)
		{
			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Clear(c, element);
		}

		public void Insert(Guid microService, Guid component, List<IDevelopmentComponentError> errors)
		{
			var ms = DataModel.MicroServices.Select(microService);

			if (ms == null)
				throw new SysException(SR.ErrMicroServiceNotFound);

			var c = DataModel.Components.Select(component);

			if (c == null)
				throw new SysException(SR.ErrComponentNotFound);

			Shell.GetService<IDatabaseService>().Proxy.Development.Errors.Insert(ms, c, errors);
		}
	}
}
