using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.StorageProvider.Sql
{
    internal class ResourceGroupWriter : Writer
    {
        public ResourceGroupWriter(IServerResourceGroup resourceGroup, IDataTransaction transaction)
        : base(transaction)
        {
            ResourceGroup = resourceGroup;
        }

        public ResourceGroupWriter(IServerResourceGroup resourceGroup, string commandText)
            : base(commandText)
        {
            ResourceGroup = resourceGroup;
        }

        public ResourceGroupWriter(IServerResourceGroup resourceGroup, string commandText, IDataTransaction transaction)
            : base(commandText, transaction)
        {
            ResourceGroup = resourceGroup;
        }

        public ResourceGroupWriter(IServerResourceGroup resourceGroup, string commandText, System.Data.CommandType type)
            : base(commandText, type)
        {
            ResourceGroup = resourceGroup;
        }

        public ResourceGroupWriter(IServerResourceGroup resourceGroup, string commandText, System.Data.CommandType type, IDataTransaction transaction)
            : base(commandText, type, transaction)
        {
            ResourceGroup = resourceGroup;
        }

        private IServerResourceGroup ResourceGroup { get; }

        protected override string ConnectionString => string.IsNullOrWhiteSpace(ResourceGroup.ConnectionString) ? DefaultConnectionString : ResourceGroup.ConnectionString;
    }
}
