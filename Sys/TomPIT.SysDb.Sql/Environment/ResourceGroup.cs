using System;
using TomPIT.Data.Sql;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Environment
{
    internal class ResourceGroup : PrimaryKeyRecord, IServerResourceGroup
    {
        public string Name { get; set; }

        public Guid Token { get; set; }

        public Guid StorageProvider { get; set; }
        public string ConnectionString { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            Name = GetString("name");
            Token = GetGuid("token");
            StorageProvider = GetGuid("storage_provider");
            ConnectionString = GetString("connection_string");
        }
    }
}
