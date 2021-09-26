namespace Microframework.Core.Interfaces
{
    public interface ILogger
    {
        void LogInformation(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
}