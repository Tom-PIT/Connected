using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using TomPIT.Analytics;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Analytics;

namespace TomPIT.SysDb.Sql.Analytics
{
	internal class MruHandler : IMruHandler
	{
		public void Delete(IMru item)
		{
			var w = new Writer("tompit.mru_del");

			w.CreateParameter("@token", item.Token);

			w.Execute();
		}

		public int Modify(Guid token, int type, string primaryKey, AnalyticsEntity entity, string entityPrimaryKey, List<string> tags, DateTime date)
		{
			var a = new JArray();

			foreach (var tag in tags)
			{
				a.Add(new JObject
				{
					{"tag", tag }
				});
			}

			var w = new Writer("tompit.mru_mdf");

			w.CreateParameter("@type", type);
			w.CreateParameter("@primary_key", primaryKey);
			w.CreateParameter("@entity_type", entity);
			w.CreateParameter("@entity_primary_key", entityPrimaryKey);
			w.CreateParameter("@tags", a);
			w.CreateParameter("@date", date);
			w.CreateParameter("@token", token);

			w.Execute();

			return w.Result;
		}

		public List<IMru> Query(AnalyticsEntity entity, string entityPrimaryKey, List<string> tags)
		{
			var a = new JArray();

			foreach (var tag in tags)
			{
				a.Add(new JObject
				{
					{"tag", tag }
				});
			}

			var r = new Reader<Mru>("tompit.mru_que");

			r.CreateParameter("@entity_type", entity);
			r.CreateParameter("@entity_primary_key", entityPrimaryKey);
			r.CreateParameter("@tags", a);

			var items = r.Execute();
			var result = new List<IMru>();

			foreach (var item in items)
			{
				if (result.FirstOrDefault(f => f.Token == item.Token) == null && IsComplete(item, items, tags))
					result.Add(item);
			}

			return result;
		}

		private bool IsComplete(Mru mru, List<Mru> items, List<string> tags)
		{
			foreach (var tag in tags)
			{
				if (items.FirstOrDefault(f => f.Token == mru.Token && string.Compare(tag, f.Tag, true) == 0) == null)
					return false;
			}

			return true;
		}
	}
}
