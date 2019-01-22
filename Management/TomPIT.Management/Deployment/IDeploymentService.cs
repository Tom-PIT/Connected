namespace TomPIT.Deployment
{
	public interface IDeploymentService
	{
		bool IsLogged { get; }
		bool IsPublisher { get; }
		void LogIn(string userName, string password, bool permanent);

	}
}
