namespace TomPIT.Cdn
{
    public interface ICdnClientNotificationProxy
    {
        void Notify(string token, string method, object arguments);
    }
}
