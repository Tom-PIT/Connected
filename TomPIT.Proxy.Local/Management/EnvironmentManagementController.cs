using System;
using TomPIT.Exceptions;
using TomPIT.Proxy.Management;
using TomPIT.Security;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local.Management;
internal class EnvironmentManagementController : IEnvironmentManagementController
{
    public void Setup(string userName, string password, string firstName, string lastName, string email, string description, string pin, string mobile, string phone)
    {
        var ev = DataModel.EnvironmentVariables.Select("Setup state");

        if (ev is not null && Types.Convert<int>(ev.Value) != 0)
            throw new BadRequestException("Environment already set.");

        var id = DataModel.Users.Insert(userName, email, UserStatus.Active, firstName, lastName,
            description, pin, Guid.Empty, null, true, mobile, phone, DateTime.MinValue, string.Empty);

        DataModel.Users.UpdatePassword(id.ToString(), null, password);

        var u = DataModel.Users.Resolve(userName);

        DataModel.Membership.Insert(u.Token, SecurityUtils.FullControlRole);
    }
}
