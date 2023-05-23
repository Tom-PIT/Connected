using TomPIT.Proxy.Management;

namespace TomPIT.Proxy.Remote.Management;
internal class EnvironmentManagementController : IEnvironmentManagementController
{
    private const string Controller = "EnvironmentManagement";

    public void Setup(string userName, string password, string firstName, string lastName, string email, string description, string pin, string mobile, string phone)
    {
        Connection.Post(Connection.CreateUrl(Controller, "Setup"), new
        {
            userName,
            password,
            firstName,
            lastName,
            email,
            description,
            pin,
            mobile,
            phone
        });
    }
}
