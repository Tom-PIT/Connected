using System.Collections.Generic;

namespace TomPIT.Data
{
	internal class EntityState
	{
		private List<OperationState> _operations = null;
		public bool Initialized { get; set; }
		public bool Valid { get; set; }
		public ModelSchema Schema { get; set; }

		public bool IsInitializing { get; set; }

		public List<OperationState> Operations
		{
			get
			{
				if (_operations == null)
					_operations = new List<OperationState>();

				return _operations;
			}
		}
	}
}
