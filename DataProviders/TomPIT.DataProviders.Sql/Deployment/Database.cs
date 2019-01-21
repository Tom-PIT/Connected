using System.Collections.Generic;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class Database : IDatabase
	{
		private List<ITable> _tables = null;
		private List<IView> _views = null;
		private List<IRoutine> _routines = null;

		public List<ITable> Tables
		{
			get
			{
				if (_tables == null)
					_tables = new List<ITable>();

				return _tables;
			}
		}

		public List<IView> Views
		{
			get
			{
				if (_views == null)
					_views = new List<IView>();

				return _views;
			}
		}

		public List<IRoutine> Routines
		{
			get
			{
				if (_routines == null)
					_routines = new List<IRoutine>();

				return _routines;
			}
		}
	}
}
