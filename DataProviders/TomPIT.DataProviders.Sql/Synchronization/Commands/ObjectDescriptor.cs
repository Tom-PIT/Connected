using System.Collections.Generic;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ObjectDescriptor
	{
		private ObjectMetaData _metaData = null;
		private List<ObjectColumn> _columns = null;
		private ObjectIdentity _identity = null;
		private ObjectRowGuid _rowGuid = null;
		private ObjectFileGroup _fileGroup = null;
		private List<ObjectIndex> _indexes = null;
		private List<ObjectConstraint> _constraints = null;

		public ObjectFileGroup FileGroup
		{
			get
			{
				if (_fileGroup == null)
					_fileGroup = new ObjectFileGroup();

				return _fileGroup;
			}
		}

		public ObjectRowGuid RowGuid
		{
			get
			{
				if (_rowGuid == null)
					_rowGuid = new ObjectRowGuid();

				return _rowGuid;
			}
		}

		public ObjectIdentity Identity
		{
			get
			{
				if (_identity == null)
					_identity = new ObjectIdentity();

				return _identity;
			}
		}

		public ObjectMetaData MetaData
		{
			get
			{
				if (_metaData == null)
					_metaData = new ObjectMetaData();

				return _metaData;
			}
		}

		public List<ObjectColumn> Columns
		{
			get
			{
				if (_columns == null)
					_columns = new List<ObjectColumn>();

				return _columns;
			}
		}

		public List<ObjectIndex> Indexes
		{
			get
			{
				if (_indexes == null)
					_indexes = new List<ObjectIndex>();

				return _indexes;
			}
		}

		public List<ObjectConstraint> Constraints
		{
			get
			{
				if (_constraints == null)
					_constraints = new List<ObjectConstraint>();

				return _constraints;
			}
		}
	}
}
