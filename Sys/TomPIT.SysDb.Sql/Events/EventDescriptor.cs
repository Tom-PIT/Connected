using System;
using TomPIT.Cdn;
using TomPIT.Data.Sql;

namespace TomPIT.SysDb.Sql.Events
{
    internal class EventDescriptor : DatabaseRecord, IEventDescriptor
    {
        public Guid Identifier { get; set; }
        public string Arguments { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public string Callback { get; set; }
        public Guid MicroService { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            Identifier = GetGuid("identifier");

            if (IsDefined("arguments"))
                Arguments = GetString("arguments");

            Name = GetString("name");
            Created = GetDate("created");
            Callback = GetString("callback");
            MicroService = GetGuid("service");
        }
    }
}
