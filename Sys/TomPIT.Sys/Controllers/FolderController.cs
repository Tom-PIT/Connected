using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class FolderController : SysController
	{
		[HttpGet]
		public List<IFolder> QueryForMicroService(Guid microService)
		{
			return DataModel.Folders.Query(microService);
		}

		[HttpGet]
		public List<IFolder> Query()
		{
			return DataModel.Folders.Query();
		}

		[HttpGet]
		public IFolder SelectByToken(Guid token)
		{
			return DataModel.Folders.Select(token);
		}

		[HttpGet]
		public IFolder Select(Guid microService, string name)
		{
			return DataModel.Folders.Select(microService, name);
		}
	}
}
