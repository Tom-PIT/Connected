using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Security;
using TomPIT.Security;

namespace TomPIT.Management.Dom
{
	internal class AuthenticationTokenElement : TransactionElement
	{
		public AuthenticationTokenElement(IDomElement parent, IAuthenticationToken token) : base(parent)
		{
			Token = token;
			Title = Token.ToString();
			Id = Token.Token.ToString();
		}

		public IAuthenticationToken Token { get; }
		public override object Component => Token;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IAuthenticationTokenManagementService>().Update(Token.Token, Token.User, Token.Name, Token.Description,
				Token.Key, Token.Claims, Token.Status, Token.ValidFrom, Token.ValidTo, Token.StartTime, Token.EndTime, Token.IpRestrictions);

			return true;
		}
	}
}
