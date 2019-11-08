namespace TomPIT.Security
{
	public interface IAuthorizationNotification
	{
		void NotifyMembershipAdded(object sender, MembershipEventArgs e);
		void NotifyMembershipRemoved(object sender, MembershipEventArgs e);
		void NotifyPermissionAdded(object sender, PermissionEventArgs e);
		void NotifyPermissionRemoved(object sender, PermissionEventArgs e);
		void NotifyPermissionChanged(object sender, PermissionEventArgs e);
	}
}
