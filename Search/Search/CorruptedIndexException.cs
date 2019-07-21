using System;

namespace TomPIT.Search
{
    internal class CorruptedIndexException : Exception
    {
        public CorruptedIndexException(Guid catalog)
        {
            Catalog = catalog;
        }

        public Guid Catalog { get; }
    }
}