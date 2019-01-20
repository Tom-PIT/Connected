using Newtonsoft.Json;

namespace TomPIT.Sys.Services
{
	internal class ServerSysAuthentication : IServerSysAuthentication
	{
		private IServerSysJwToken _token = null;

		[JsonProperty(PropertyName = "jwToken")]
		public IServerSysJwToken JwToken
		{
			get
			{
				if (_token == null)
					_token = new ServerSysJwToken();

				return _token;
			}
		}
	}
}
