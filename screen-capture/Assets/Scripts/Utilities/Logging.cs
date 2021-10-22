public enum LogLevel {
    None,
    Error,
    All
}

/// <summary>
/// A logging utility to show / surpress logs for classes
/// Usage: Logging logging = new Logging("NameOfYourClass") { logLevel = LogLevel.Error };
/// </summary>
public class Logging {
    public string id = string.Empty;
    public LogLevel logLevel = LogLevel.Error;

    public Logging(string id) {
        this.id = id;
    }

    public void Log(string message) {
        if (logLevel != LogLevel.All) return;
        Console.Log($"{id}: {message}");
    }

    public void LogWarning(string message) {
        if (logLevel == LogLevel.None) return;
        Console.LogWarning($"{id}: {message}");
    }
}
