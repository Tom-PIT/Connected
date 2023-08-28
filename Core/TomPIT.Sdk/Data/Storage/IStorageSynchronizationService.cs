using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TomPIT.Data.Storage;
public interface IStorageSynchronizationService
{
	Task Synchronize(List<Type>? entities);
}
