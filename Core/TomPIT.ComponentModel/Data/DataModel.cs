using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;

namespace TomPIT.Data
{
    public abstract class DataModel
    {
        protected DataModel(IDataModelContext e)
        {
            this.e = e;
        }
        protected IDataModelContext e { get; }
    }
}
