using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TomPIT.Distributed
{
    public interface IWorkerService
    {
        void Run(Guid configuration);
    }
}
