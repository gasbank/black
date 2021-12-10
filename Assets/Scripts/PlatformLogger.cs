using ConditionalDebug;

class PlatformLogger : IPlatformLogger {
    public void Log(object message) {
        ConDebug.Log(message);
    }

    public void LogFormat(string format, params object[] args) {
        ConDebug.LogFormat(format, args);
    }
}