using System.Data;
using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql;

internal class ResourceGroupReader<T> : Reader<T> where T : DatabaseRecord, new()
{
    public ResourceGroupReader(IServerResourceGroup resourceGroup, string commandText)
    : base(commandText)
    {
        ResourceGroup = resourceGroup;
    }

    public ResourceGroupReader(IServerResourceGroup resourceGroup, string commandText, CommandType type)
        : base(commandText, type)
    {
        ResourceGroup = resourceGroup;
    }

    public ResourceGroupReader(IServerResourceGroup resourceGroup, string commandText, IDataTransaction transaction)
        : base(commandText, transaction)
    {
        ResourceGroup = resourceGroup;
    }

    public ResourceGroupReader(IServerResourceGroup resourceGroup, string commandText, CommandType commandType, IDataTransaction transaction)
        : base(commandText, commandType, transaction)
    {
        ResourceGroup = resourceGroup;
    }

    private IServerResourceGroup ResourceGroup { get; }

    protected override string ConnectionString => string.IsNullOrWhiteSpace(ResourceGroup.ConnectionString) ? DefaultConnectionString : ResourceGroup.ConnectionString;
}
