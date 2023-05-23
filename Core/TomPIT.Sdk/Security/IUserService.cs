using System;
using System.Collections.Generic;
using TomPIT.Connectivity;

namespace TomPIT.Security
{
    public delegate void UserChangedHandler(ITenant tenant, UserEventArgs e);

    public interface IUserService
    {
        event UserChangedHandler UserChanged;

        List<IUser> Query();
        IUser Select(string qualifier);

        IUser SelectByAuthenticationToken(Guid token);
        IUser SelectBySecurityCode(string securityCode);

        void Logout(int user);
        void ChangePassword(Guid user, string existingPassword, string password);
        void ChangeAvatar(Guid user, byte[] contentBytes, string contentType, string fileName);

        public Guid Insert(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
            string timezone, bool notificationsEnabled, string mobile, string phone, string password, string securityCode);

        public void Update(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
            string timezone, bool notificationsEnabled, string mobile, string phone, string securityCode);
    }
}