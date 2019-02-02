using TomPIT.Security;

namespace TomPIT.Dom
{
	internal class AuthenticationTokenElement : TransactionElement
	{
		public AuthenticationTokenElement(IDomElement parent, IAuthenticationToken token) : base(parent)
		{
			Token = token;
			Title = Token.ToString();
			Id = Token.Token.AsString();
		}

		public IAuthenticationToken Token { get; }
		public override object Component => Token;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IAuthenticationTokenManagementService>().Update(Token.Token, Token.User, Token.Name, Token.Description, Token.Key, Token.Claims, Token.Status, Token.ValidFrom, Token.ValidTo,
				Token.StartTime, Token.EndTime, Token.IpRestrictions);

			return true;
		}
	}
}
