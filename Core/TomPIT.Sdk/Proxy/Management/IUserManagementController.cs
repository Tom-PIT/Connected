using System;
using TomPIT.Security;

namespace TomPIT.Proxy.Management
{
    public interface IUserManagementController
    {
        void ChangeAvatar(Guid user, Guid avatar);
        void ChangePassword(Guid user, string existingPassword, string password);

        Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone,
            bool notificationsEnabled, string mobile, string phone, DateTime passwordChange, string securityCode);
        void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language, string timezone,
            bool notificationsEnabled, string mobile, string phone, DateTime passwordChange, string securityCode);
        void ResetPassword(Guid token);
        void Delete(Guid token);
    }
}
