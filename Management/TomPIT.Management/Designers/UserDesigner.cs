using Newtonsoft.Json.Linq;
using System;
using System.IO;
using TomPIT.ActionResults;
using TomPIT.Annotations;
using TomPIT.Dom;
using TomPIT.Ide;
using TomPIT.Security;
using TomPIT.Storage;

namespace TomPIT.Designers
{
	public class UserDesigner : DomDesigner<UserElement>
	{
		public UserDesigner(IEnvironment environment, UserElement element) : base(environment, element)
		{
		}

		public override string View => "~/Views/Ide/Designers/User.cshtml";
		public override object ViewModel => this;

		protected override IDesignerActionResult OnAction(JObject data, string action)
		{
			if (string.Compare(action, "upload", true) == 0)
				return Upload();
			else if (string.Compare(action, "resetAvatar", true) == 0)
				return ResetAvatar(data);
			else if (string.Compare(action, "resetPassword", true) == 0)
				return ResetPassword(data);

			return base.OnAction(data, action);
		}

		public IUser User { get { return Owner.Component as IUser; } }

		private IDesignerActionResult ResetPassword(JObject data)
		{
			Connection.GetService<IUserManagementService>().ResetPassword(User.Token);

			return Result.EmptyResult(this);
		}

		private IDesignerActionResult ResetAvatar(JObject data)
		{
			Connection.GetService<IUserManagementService>().ChangeAvatar(User.Token, null, null, null);

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}

		private IDesignerActionResult Upload()
		{
			var files = Shell.HttpContext.Request.Form.Files;

			if (files == null || files.Count == 0)
				return Result.EmptyResult(ViewModel);

			var file = files[0];

			using (var s = new MemoryStream())
			{
				file.CopyTo(s);

				var buffer = new byte[file.Length];

				s.Seek(0, SeekOrigin.Begin);
				s.Read(buffer, 0, buffer.Length);

				Connection.GetService<IUserManagementService>().ChangeAvatar(User.Token, buffer, file.ContentType, System.IO.Path.GetFileName(file.FileName));
			};

			return Result.SectionResult(ViewModel, EnvironmentSection.Designer);
		}

		public string AvatarUrl
		{
			get
			{
				if (User == null || User.Avatar == Guid.Empty)
					return null;

				var b = Connection.GetService<IStorageService>().Select(User.Avatar);

				if (b == null)
					return null;

				return string.Format("_sys/avatar/{0}/{1}", b.Token, b.Version);
			}
		}
	}
}
