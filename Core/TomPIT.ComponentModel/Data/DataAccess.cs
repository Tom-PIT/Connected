using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Data
{
    public abstract class DataAccess
    {
        protected DataAccess(OperationArguments e)
        {
            this.e = e;
        }
        protected OperationArguments e { get; }
    }
}
