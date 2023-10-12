using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.ComponentModel;
using TomPIT.Data.Sql;
using TomPIT.Environment;
using TomPIT.SysDb.Development;

namespace TomPIT.SysDb.Sql.Development;

public class MicroServiceHandler : IMicroServiceHandler
{
	private const string SelectProcedure = "tompit.service_sel";

	public void Delete(IMicroService service)
	{
		using var w = new Writer("tompit.service_del");

		w.CreateParameter("@id", service.GetId());

		w.Execute();
	}

	public void Insert(Guid token, string name, string url, MicroServiceStages supportedStages, IResourceGroup resourceGroup, Guid template, string version, string commit)
	{
		using var p = new Writer("tompit.service_ins");

		p.CreateParameter("@name", name);
		p.CreateParameter("@url", url);
		p.CreateParameter("@supported_stages", supportedStages);
		p.CreateParameter("@resource_group", resourceGroup.GetId());
		p.CreateParameter("@template", template);
		p.CreateParameter("@token", token);
		p.CreateParameter("@version", version, true);
		p.CreateParameter("@commit", commit, true);

		p.Execute();
	}

	public List<IMicroService> Query()
	{
		using var r = new Reader<MicroService>("tompit.service_que");

		return r.Execute().ToList<IMicroService>();
	}

	public IMicroService Select(int id)
	{
		using var p = new Reader<MicroService>(SelectProcedure);

		p.CreateParameter("@id", id);

		return p.ExecuteSingleRow();
	}

	public IMicroService SelectByUrl(string url)
	{
		using var p = new Reader<MicroService>(SelectProcedure);

		p.CreateParameter("@url", url);

		return p.ExecuteSingleRow();

	}

	public IMicroService Select(Guid token)
	{
		using var p = new Reader<MicroService>(SelectProcedure);

		p.CreateParameter("@token", token);

		return p.ExecuteSingleRow();
	}

	public IMicroService Select(string name)
	{
		using var p = new Reader<MicroService>(SelectProcedure);

		p.CreateParameter("@name", name);

		return p.ExecuteSingleRow();
	}

	public void Update(IMicroService microService, string name, string url, MicroServiceStages supportedStages, Guid template, IResourceGroup resourceGroup, string version, string commit)
	{
		using var p = new Writer("tompit.service_upd");

		p.CreateParameter("@name", name);
		p.CreateParameter("@url", url);
		p.CreateParameter("@id", microService.GetId());
		p.CreateParameter("@supported_stages", supportedStages);
		p.CreateParameter("@template", template);
		p.CreateParameter("@resource_group", resourceGroup.GetId());
		p.CreateParameter("@version", version, true);
		p.CreateParameter("@commit", commit, true);

		p.Execute();
	}
}