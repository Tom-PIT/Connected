using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.ComponentModel.Apis;
using TomPIT.Services;

namespace TomPIT.Data
{
    public abstract class DataModel
    {
        protected DataModel(IDataModelContext context)
        {
            Context = context;
        }
        protected IDataModelContext Context { get; }
    }
}
