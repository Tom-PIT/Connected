using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TomPIT.ComponentModel;
public interface IMultiFileElement
{
	Task ProcessCreated();
	Task ProcessChanged();
	Task ProcessDeleted();
	Task ProcessRestored();

	Task<List<KeyValuePair<Guid, int>>> QueryAdditionalFiles();
}
