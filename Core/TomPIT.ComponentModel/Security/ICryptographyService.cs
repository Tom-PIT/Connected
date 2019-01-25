namespace TomPIT.Security
{
	public interface ICryptographyService
	{
		string Encrypt(string plainText);
		string Decrypt(string cipherText);

		string Hash(string value);
		bool VerifyHash(string value, string existing);
	}
}
