using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Design;
using TomPIT.Designers;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Storage;

namespace TomPIT.Application.Design.Designers
{
	internal class AssemblyUploadDesigner : DomDesigner<ReflectionElement>, IUploadDesigner
	{
		public AssemblyUploadDesigner(IEnvironment environment, ReflectionElement element) : base(environment, element)
		{
			Resource = element.Component as IUploadResource;
		}

		public IUploadResource Resource { get; }

		public override string View => "~/Views/Ide/Designers/Upload.cshtml";
		public override object ViewModel => this;

		public string FileExtension => ".dll";

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "upload", true) == 0)
				return Upload();

			return base.OnAction(data, action);
		}

		private IDesignerActionResult Upload()
		{
			var files = Environment.Context.GetHttpRequest().Form.Files;

			if (files == null || files.Count == 0)
				return Result.EmptyResult(ViewModel);

			var file = files[0];

			var ms = Connection.GetService<IMicroServiceService>().Select(Environment.Context.MicroService());

			var b = new Blob
			{
				ContentType = file.ContentType,
				FileName = System.IO.Path.GetFileName(file.FileName),
				MicroService = ms.Token,
				Size = Convert.ToInt32(file.Length),
				PrimaryKey = Element.Id,
				ResourceGroup = ms.ResourceGroup
			};

			using (var s = new MemoryStream())
			{
				file.CopyTo(s);

				var buffer = new byte[file.Length];

				s.Seek(0, SeekOrigin.Begin);
				s.Read(buffer, 0, buffer.Length);

				var id = Connection.GetService<IStorageService>().Upload(b, buffer, StoragePolicy.Singleton);

				Resource.Blob = id;
				Resource.FileName = b.FileName;

				Connection.GetService<IComponentDevelopmentService>().Update(Resource.Configuration());
			};

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}
	}
}
