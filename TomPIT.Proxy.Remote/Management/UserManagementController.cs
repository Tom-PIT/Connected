using System;
using TomPIT.Proxy.Management;
using TomPIT.Security;

namespace TomPIT.Proxy.Remote.Management
{
    internal class UserManagementController : IUserManagementController
    {
        private const string Controller = "UserManagement";

        public void ChangeAvatar(Guid user, Guid avatar)
        {
            Connection.Post(Connection.CreateUrl(Controller, "ChangeAvatar"), new
            {
                user,
                avatar
            });
        }

        public void ChangePassword(Guid user, string existingPassword, string password)
        {
            Connection.Post(Connection.CreateUrl(Controller, "ChangePassword"), new
            {
                user,
                newPassword = password,
                existingPassword
            });

        }

        public void Delete(Guid token)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
            {
                user = token
            });
        }

        public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationEnabled, string mobile, string phone, DateTime passwordChange, string securityCode)
        {
            return Connection.Post<Guid>(Connection.CreateUrl(Controller, "Insert"), new
            {
                email,
                loginName,
                firstName,
                description,
                pin,
                language,
                timezone,
                notificationEnabled,
                mobile,
                phone,
                securityCode
            });
        }

        public void ResetPassword(Guid token)
        {
            Connection.Post(Connection.CreateUrl(Controller, "ResetPassword"), new
            {
                user = token
            });
        }

        public void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone, bool notificationsEnabled, string mobile, string phone, DateTime passwordChange, string securityCode)
        {
            Connection.Post(Connection.CreateUrl(Controller, "Update"), new
            {
                email,
                loginName,
                firstName,
                lastName,
                description,
                status,
                user = token,
                pin,
                language,
                timezone,
                notificationEnabled = notificationsEnabled,
                mobile,
                phone,
                passwordChange,
                securityCode
            });
        }
    }
}
