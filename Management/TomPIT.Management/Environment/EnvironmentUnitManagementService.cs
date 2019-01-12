using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Connectivity;

namespace TomPIT.Environment
{
	internal class EnvironmentUnitManagementService : IEnvironmentUnitManagementService
	{
		public EnvironmentUnitManagementService(ISysConnection connection)
		{
			Connection = connection;
		}

		private ISysConnection Connection { get; }

		public void Delete(Guid unit)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Delete");
			var e = new JObject
			{
				{"token", unit }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyRemoved(this, new EnvironmentUnitEventArgs(unit));
		}

		public Guid Insert(string name, Guid parent, int ordinal)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Insert");
			var e = new JObject
			{
				{"name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			var id = Connection.Post<Guid>(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(id));

			return id;
		}

		public void Move(Guid unit, Guid previous, Guid parent)
		{
			var item = Connection.GetService<IEnvironmentUnitService>().Select(unit);
			var ids = new List<Guid>();

			if (item == null)
				throw new RuntimeException(SR.ErrEnvironmentUnitNotFound);

			var p = new JObject();
			var a = new JArray();

			p.Add("data", a);

			var siblings = Connection.GetService<IEnvironmentUnitService>().Query(parent).OrderBy(f => f.Ordinal).ThenBy(f => f.Name).ToList();

			if (previous != Guid.Empty)
				siblings = siblings.SkipWhile(f => f.Token != previous).ToList();

			int index = -1;

			if (item.Parent == parent)
			{
				var d = siblings.First(f => f.Token == item.Token);

				siblings.Remove(d);
			}

			index = previous == Guid.Empty ? 0 : 1;

			siblings.Insert(index, item);

			if (previous == Guid.Empty)
				index = 0;
			else
				index = siblings[0].Ordinal;


			if (index == -1)
				index = siblings[0].Ordinal;

			foreach (var i in siblings)
			{
				var prt = i.Token == unit ? parent : i.Parent;

				if (i.Ordinal != index || i.Token == unit)
				{
					a.Add(new JObject
					{
						{"token", i.Token },
						{"name", i.Name },
						{"ordinal", index },
						{"parent", prt }
				});

					ids.Add(i.Token);
				}

				index++;
			}

			if (item.Parent != parent)
			{
				siblings = Connection.GetService<IEnvironmentUnitService>().Query(item.Parent).OrderBy(f => f.Ordinal).ThenBy(f => f.Name).ToList();
				index = 0;

				foreach (var i in siblings)
				{
					if (i.Token == unit)
						continue;

					if (i.Ordinal != index)
					{
						a.Add(new JObject
						{
							{"token", i.Token },
							{"name", i.Name },
							{"ordinal", index },
							{"parent", i.Parent }
						});

						ids.Add(i.Token);
					}

					index++;
				}
			}

			var u = Connection.CreateUrl("EnvironmentUnitManagement", "UpdateBatch");

			Connection.Post(u, p);

			foreach (var i in ids)
			{
				if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
					n.NotifyChanged(this, new EnvironmentUnitEventArgs(i));
			}
		}

		public void Update(Guid unit, string name, Guid parent, int ordinal)
		{
			var u = Connection.CreateUrl("EnvironmentUnitManagement", "Update");
			var e = new JObject
			{
				{"token", unit},
				{ "name", name },
				{"parent", parent },
				{"ordinal", ordinal }
			};

			Connection.Post(u, e);

			if (Connection.GetService<IEnvironmentUnitService>() is IEnvironmentUnitNotification n)
				n.NotifyChanged(this, new EnvironmentUnitEventArgs(unit));
		}
	}
}
