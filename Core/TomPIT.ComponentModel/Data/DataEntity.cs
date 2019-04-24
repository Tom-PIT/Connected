using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Data
{
    public abstract class DataEntity
    {
        protected virtual T GetValue<T>(string propertyName)
        {
            return default;
        }
        protected virtual T GetValue<T>(string propertyName, T defaultValue)
        {
            return defaultValue;
        }
        protected virtual void SetValue<T>(string propertyName, T value)
        {

        }

        protected virtual void OnDatabind()
        {

        }
    }
}
