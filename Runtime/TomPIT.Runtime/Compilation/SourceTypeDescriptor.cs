using System;

namespace TomPIT.Compilation
{
    public class SourceTypeDescriptor
    {
        public Guid Component { get; set; }
        public string ContainingType { get; set; }
        public string TypeName { get; set; }
        public Guid Script { get; set; }
    }
}
