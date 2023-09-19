using System;
using TomPIT.Connectivity;
using TomPIT.Security;

namespace TomPIT.Management.Security
{
    internal class UserManagementService : TenantObject, IUserManagementService
    {
        public UserManagementService(ITenant tenant) : base(tenant)
        {

        }

        public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string password, string pin, Guid language, string timezone,
            bool notificationEnabled, string mobile, string phone, string securityCode)
        {
            var id = Instance.SysProxy.Management.Users.Insert(loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationEnabled, mobile,
                phone, DateTime.MinValue, securityCode);

            if (!string.IsNullOrWhiteSpace(password))
                Instance.SysProxy.Management.Users.ChangePassword(id, null, password);

            return id;
        }

        public void ResetPassword(Guid user)
        {
            Instance.SysProxy.Management.Users.ResetPassword(user);
        }

        public void Delete(Guid user)
        {
            throw new Exception(SR.ErrUserDeleteUnsupported);

            Instance.SysProxy.Management.Users.Delete(user);

            if (Tenant.GetService<IUserService>() is IUserNotification n)
                n.NotifyChanged(this, new UserEventArgs(user));
        }

        public void Update(Guid user, string loginName, string email, UserStatus status, string firstName, string lastName,
            string description, string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, string securityCode)
        {
            var existing = Tenant.GetService<IUserService>().Select(user.ToString());

            if (existing is null)
                return;

            Instance.SysProxy.Management.Users.Update(user, loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationEnabled, mobile, phone,
                existing.PasswordChange, securityCode);

            if (Tenant.GetService<IUserService>() is IUserNotification n)
                n.NotifyChanged(this, new UserEventArgs(user));
        }
    }
}
