public interface IPlatformLogger {
    void Log(object message);
    void LogFormat(string format, params object[] args);
}