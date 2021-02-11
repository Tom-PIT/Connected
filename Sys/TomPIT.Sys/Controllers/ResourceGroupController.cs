using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TomPIT.Environment;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Controllers
{
	public class ResourceGroupController : SysController
	{
		[HttpGet]
		public List<IResourceGroup> Query()
		{
			return DataModel.ResourceGroups.Query().ToList<IResourceGroup>();
		}

		[HttpGet]
		public IResourceGroup Select(Guid resourceGroup)
		{
			return DataModel.ResourceGroups.Select(resourceGroup);
		}

		[HttpGet]
		public IResourceGroup SelectByName(string resourceGroup)
		{
			return DataModel.ResourceGroups.Select(resourceGroup);
		}
	}
}
