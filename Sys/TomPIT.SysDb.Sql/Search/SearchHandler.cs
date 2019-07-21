using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Search;
using TomPIT.SysDb.Search;

namespace TomPIT.SysDb.Sql.Search
{
	internal class SearchHandler : ISearchHandler
	{
		public void Delete(IIndexRequest d)
		{
			var w = new Writer("tompit.search_del");

			w.CreateParameter("@id", d.GetId());

			w.Execute();
		}

		public void DeleteState(Guid catalog)
		{
			var w = new Writer("tompit.search_catalog_state_del");

			w.CreateParameter("@catalog", catalog);

			w.Execute();
		}

		public void Insert(IMicroService microService, string catalog, Guid identifier, DateTime created, string arguments)
		{
			var w = new Writer("tompit.search_ins");

			w.CreateParameter("@service", microService.Token);
			w.CreateParameter("@catalog", catalog);
			w.CreateParameter("@identifier", identifier);
			w.CreateParameter("@created", created);
			w.CreateParameter("@arguments", arguments, true);

			w.Execute();
		}

		public void InvalidateState(Guid catalog)
		{
			var w = new Writer("tompit.search_catalog_state_ins");

			w.CreateParameter("@catalog", catalog);
			w.CreateParameter("@status", CatalogStateStatus.Pending);

			w.Execute();
		}

		public List<IIndexRequest> Query()
		{
			return new Reader<IndexRequest>("tompit.search_que").Execute().ToList<IIndexRequest>();
		}

		public IIndexRequest Select(Guid identifier)
		{
			var r = new Reader<IndexRequest>("tompit.search_sel");

			r.CreateParameter("@identifier", identifier);

			return r.ExecuteSingleRow();
		}

		public ICatalogState SelectState(Guid catalog)
		{
			var r = new Reader<CatalogState>("tompit.search_catalog_state_sel");

			r.CreateParameter("@catalog", catalog);

			return r.ExecuteSingleRow();
		}

		public void UpdateState(Guid catalog, CatalogStateStatus status)
		{
			var w = new Writer("tompit.search_catalog_state_upd");

			w.CreateParameter("@catalog", catalog);
			w.CreateParameter("@status", status);

			w.Execute();
		}
	}
}
