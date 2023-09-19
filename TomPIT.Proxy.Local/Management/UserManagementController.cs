using System;
using TomPIT.Proxy.Management;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management
{
    internal class UserManagementController : IUserManagementController
    {
        public void ChangeAvatar(Guid user, Guid avatar)
        {
            DataModel.Users.ChangeAvatar(user, avatar);
        }

        public void ChangePassword(Guid user, string existingPassword, string password)
        {
            DataModel.Users.UpdatePassword(user.ToString(), existingPassword, password);
        }

        public void Delete(Guid token)
        {
            DataModel.Users.Delete(token);
        }

        public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled, string mobile, string phone, DateTime passwordChange, string securityCode)
        {
            return DataModel.Users.Insert(loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationsEnabled, mobile, phone, passwordChange, securityCode);
        }

        public void ResetPassword(Guid token)
        {
            DataModel.Users.ResetPassword(token.ToString(), null);
        }

        public void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled, string mobile, string phone, DateTime passwordChange, string securityCode)
        {
            DataModel.Users.Update(token, loginName, email, status, firstName, lastName, description, pin, language, timezone, notificationsEnabled, mobile, phone, passwordChange, securityCode);
        }
    }
}
