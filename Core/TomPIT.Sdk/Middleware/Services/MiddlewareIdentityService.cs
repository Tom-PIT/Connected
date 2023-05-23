using System;
using TomPIT.Security;

namespace TomPIT.Middleware.Services
{
    internal class MiddlewareIdentityService : MiddlewareObject, IMiddlewareIdentityService
    {
        private IUser _user = null;
        private string _impersonatedUser = null;

        public MiddlewareIdentityService(IMiddlewareContext context) : base(context)
        {
        }

        public bool IsAuthenticated
        {
            get
            {
                if (Context is MiddlewareContext mc && mc.Owner != null)
                    return mc.Owner.Services.Identity.IsAuthenticated;

                if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
                    return true;

                if (Shell.HttpContext != null)
                {
                    if (Shell.HttpContext.User == null || Shell.HttpContext.User.Identity == null)
                        return false;

                    return Shell.HttpContext.User.Identity.IsAuthenticated;
                }

                return false;
            }
        }

        internal string ImpersonatedUser
        {
            get
            {
                if (Context is MiddlewareContext mc && mc.Owner != null)
                    return ((MiddlewareIdentityService)mc.Owner.Services.Identity).ImpersonatedUser;

                return _impersonatedUser;
            }
            set
            {
                if (Context is MiddlewareContext mc && mc.Owner != null)
                    ((MiddlewareIdentityService)mc.Owner.Services.Identity).ImpersonatedUser = value;
                else
                {
                    _user = Context.Tenant.GetService<IUserService>().Select(value);

                    if (_user != null)
                        _impersonatedUser = value;
                    else
                        _impersonatedUser = null;
                }
            }
        }

        public IUser User
        {
            get
            {
                if (!IsAuthenticated)
                    return null;

                if (Context is MiddlewareContext mc && mc.Owner != null)
                    return mc.Owner.Services.Identity.User;

                if (_user == null)
                {
                    if (!string.IsNullOrWhiteSpace(ImpersonatedUser))
                    {
                        var ctx = Context.Tenant;

                        if (ctx != null)
                            return ctx.GetService<IUserService>().Select(ImpersonatedUser);
                    }
                    else
                        _user = MiddlewareDescriptor.Current.User;
                }
                return _user;
            }
        }

        public IUser GetUser(object qualifier)
        {
            if (qualifier == null)
                return null;

            return Context.Tenant.GetService<IUserService>().Select(qualifier == null ? string.Empty : qualifier.ToString());
        }

        public IAuthenticationResult Authenticate(string user, string password)
        {
            return Context.Tenant.GetService<IAuthorizationService>().Authenticate(user, password);
        }

        public IAuthenticationResult Authenticate(string authenticationToken)
        {
            return Context.Tenant.GetService<IAuthorizationService>().Authenticate(authenticationToken);
        }

        public Guid InsertUser(string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
            string timezone, bool notificationsEnabled, string mobile, string phone, string password = null, string securityCode = null)
        {
            var id = Context.Tenant.GetService<IUserService>().Insert(loginName, email, status, firstName, lastName, description, pin,
                language, timezone, notificationsEnabled, mobile, phone, password, securityCode);

            if (password is not null)
                Context.Tenant.GetService<IUserService>().ChangePassword(id, null, password);

            return id;
        }

        public void UpdateUser(Guid token, string loginName, string email, UserStatus status, string firstName, string lastName, string description, string pin, Guid language,
            string timezone, bool notificationsEnabled, string mobile, string phone, string securityCode = null)
        {
            Context.Tenant.GetService<IUserService>().Update(token, loginName, email, status, firstName, lastName, description, pin, language, timezone,
                notificationsEnabled, mobile, phone, securityCode);

            if (Context.Tenant.GetService<IUserService>() is IUserNotification n)
                n.NotifyChanged(this, new UserEventArgs(token));
        }

        public Guid InsertAlien(string firstName, string lastName, string email, string mobile, string phone, Guid language, string timezone)
        {
            return Context.Tenant.GetService<IAlienService>().Insert(firstName, lastName, email, mobile, phone, language, timezone);
        }

        public IAlien GetAlien(string email)
        {
            return Context.Tenant.GetService<IAlienService>().Select(email);
        }

        public IAlien GetAlienByMobile(string mobile)
        {
            return Context.Tenant.GetService<IAlienService>().SelectByMobile(mobile);
        }

        public IAlien GetAlienByPhone(string phone)
        {
            return Context.Tenant.GetService<IAlienService>().SelectByPhone(phone);
        }
    }
}
