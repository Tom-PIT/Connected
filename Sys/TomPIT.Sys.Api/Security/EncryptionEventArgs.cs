using System;

namespace TomPIT.Security
{
	public class EncryptionEventArgs : EventArgs
	{
		public string Value { get; set; }
		public string PassPhrase { get; set; }
		public string SaltValue { get; set; }
		public string HashAlgorithm { get; set; }
		public int PasswordIterations { get; set; }
		public string InitVector { get; set; }
		public int KeySize { get; set; }
	}
}
