namespace TomPIT.Proxy.Management
{
    public interface IEnvironmentManagementController
    {
        void Setup(string userName, string password, string firstName, string lastName, string email, string description, string pin, string mobile, string phone);
    }
}
