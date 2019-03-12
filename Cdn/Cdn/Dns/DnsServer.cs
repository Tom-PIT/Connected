namespace TomPIT.Cdn.Dns
{
	internal enum EncryptionType
	{
		None,
		SSL,
		TLS
	}

	internal class DnsServer
	{
		private string _host = string.Empty;
		private string _username = string.Empty;
		private string _password = string.Empty;

		private int _port = 0;
		private bool _requiresAuthentication = false;
		private EncryptionType _encType = EncryptionType.None;

		public DnsServer()
		{
			Host = "127.0.0.1";
			Port = 0;
			Username = string.Empty;
			Password = string.Empty;
		}

		public DnsServer(string host, int port)
		{
			Host = host;
			Port = port;
			Username = string.Empty;
			Password = string.Empty;
		}

		public DnsServer(string host, int port, string username, string password)
			: this(host, port)
		{
			Username = username;
			Password = password;
		}

		public DnsServer(string host, int port, string username, string password, bool RequiresAuthentication, EncryptionType EncType)
			: this(host, port, username, password)
		{

			_requiresAuthentication = RequiresAuthentication;
			_encType = EncType;
		}

		public string Username { get { return _username; } set { _username = value; } }
		public string Password { get { return _password; } set { _password = value; } }
		public string Host { get { return _host; } set { _host = value; } }
		public int Port { get { return _port; } set { _port = value; } }
		public bool RequiresAuthentication { get { return _requiresAuthentication; } set { _requiresAuthentication = value; } }
		public EncryptionType ServerEncryptionType { get { return _encType; } set { _encType = value; } }
	}
}