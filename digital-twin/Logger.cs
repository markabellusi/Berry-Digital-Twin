namespace BerryTwinProducerConsumerModel
{
public class Logger
{
    private static readonly object LockObject = new object();
    private static readonly string LogFilePath = "log.txt";

    public static void Log(string text, ConsoleColor color = ConsoleColor.White)
    {
        LogInternal(text, color);
    }
// Different methods for diferent levels of error logging
    public static void LogInfo(string text)
    {
        LogInternal($"INFO: {text}", ConsoleColor.Green);
    }

    public static void LogWarning(string text)
    {
        LogInternal($"WARNING: {text}", ConsoleColor.Yellow);
    }

    public static void LogError(string text, Exception exception = null)
    {
        LogInternal($"ERROR: {text}", ConsoleColor.Red);

        if (exception != null)
        {
            LogInternal($"Exception Details: {exception}", ConsoleColor.Red);
        }
    }

    private static void LogInternal(string text, ConsoleColor color)
    {
        lock (LockObject)
        {
            Console.ForegroundColor = color;
            string logMessage = $"[{DateTime.UtcNow:hh:mm:ss.ff}] - {text}";
            Console.WriteLine(logMessage);
            WriteToFile(logMessage);
        }
    }

    private static void WriteToFile(string logMessage)
    {
        try
        {
            // Append the log message to the log file.
            File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // Handle exceptions related to file writing.
            Console.WriteLine($"Error writing to log file: {ex.Message}");
        }
    }
    }
}
