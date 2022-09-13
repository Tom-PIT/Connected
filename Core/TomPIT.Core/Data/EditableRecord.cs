using System;
using System.ComponentModel;

namespace TomPIT.Data
{
	public abstract class EditableRecord : IEditableObject
	{
		private bool _isLocked = false;
		private readonly object _sync = new object();

		public bool IsLocked { get { return _isLocked; } private set { _isLocked = true; } }

		public void BeginEdit()
		{
			if (IsLocked)
				throw new Exception("Locked.");

			lock (_sync)
			{
				if (IsLocked)
					throw new Exception("Locked.");

				IsLocked = true;
			}
		}

		public void CancelEdit()
		{
			_isLocked = false;
		}

		public void EndEdit()
		{
			_isLocked = false;
		}
	}
}
