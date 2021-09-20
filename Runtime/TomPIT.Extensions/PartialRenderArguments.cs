using System;

namespace TomPIT
{
    public class PartialRenderArguments : EventArgs
    {
        public object Arguments { get; set; }

        public bool MergeArguments { get; set; }
    }
}