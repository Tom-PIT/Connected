using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TomPIT.Storage;
using TomPIT.Sys.Data;

namespace TomPIT.Sys.Controllers
{
	public class StorageController : SysController
	{
		[HttpPost]
		public void Commit()
		{
			var body = FromBody();

			var draft = body.Required<Guid>("draft");
			var primaryKey = body.Required<string>("primaryKey");

			DataModel.Blobs.Commit(draft, primaryKey);
		}

		[HttpPost]
		public void Delete()
		{
			var body = FromBody();

			var blob = body.Required<Guid>("blob");

			DataModel.Blobs.Delete(blob);
		}

		[HttpGet]
		public List<IBlob> Query(Guid resourceGroup, int type, string primaryKey)
		{
			return DataModel.Blobs.Query(resourceGroup, type, primaryKey);
		}

		[HttpGet]
		public List<IBlob> QueryByTopic(Guid resourceGroup, int type, string primaryKey, Guid microService, string topic)
		{
			return DataModel.Blobs.Query(resourceGroup, type, primaryKey, microService, topic);
		}

		[HttpGet]
		public List<IBlob> QueryDrafts(Guid draft)
		{
			return DataModel.Blobs.QueryDrafts(draft);
		}

		[HttpGet]
		public List<IBlob> QueryByMicroService(Guid microService)
		{
			return DataModel.Blobs.Query(microService);
		}

		[HttpGet]
		public IBlob Select(Guid blob)
		{
			return DataModel.Blobs.Select(blob);
		}

		[HttpPost]
		public Guid Upload()
		{
			var body = FromBody();

			var resourceGroup = body.Required<Guid>("resourceGroup");
			var type = body.Required<int>("type");
			var primaryKey = body.Optional("primaryKey", string.Empty);
			var microService = body.Optional("microService", Guid.Empty);
			var topic = body.Optional("topic", string.Empty);
			var fileName = body.Required<string>("fileName");
			var contentType = body.Required<string>("contentType");
			var draft = body.Optional("draft", Guid.Empty);
			var content = body.Optional("content", string.Empty);
			var policy = body.Optional("policy", StoragePolicy.Singleton);
			var token = body.Optional("token", Guid.Empty);

			return DataModel.Blobs.Upload(resourceGroup, type, primaryKey, microService, topic, fileName, contentType, draft, Convert.FromBase64String(content), policy, token);
		}

		[HttpPost]
		public void Restore()
		{
			var body = FromBody();

			var resourceGroup = body.Required<Guid>("resourceGroup");
			var type = body.Required<int>("type");
			var primaryKey = body.Required<string>("primaryKey");
			var microService = body.Optional("microService", Guid.Empty);
			var topic = body.Optional("topic", string.Empty);
			var fileName = body.Required<string>("fileName");
			var contentType = body.Required<string>("contentType");
			var content = body.Optional("content", string.Empty);
			var policy = body.Required<StoragePolicy>("policy");
			var token = body.Required<Guid>("token");
			var version = body.Required<int>("version");

			DataModel.Blobs.Restore(resourceGroup, type, primaryKey, microService, topic, fileName, contentType, Convert.FromBase64String(content), policy, token, version);
		}

		[HttpGet]
		public IBlobContent Download(Guid blob)
		{
			return DataModel.BlobsContents.Select(blob);
		}

		[HttpPost]
		public List<IBlobContent> DownloadBatch()
		{
			var body = FromBody();
			var blobs = new List<Guid>();

			foreach (var i in body["items"].Children())
				blobs.Add(Types.Convert<Guid>(i.Value<string>()));

			return DataModel.BlobsContents.Query(blobs);
		}
	}
}
