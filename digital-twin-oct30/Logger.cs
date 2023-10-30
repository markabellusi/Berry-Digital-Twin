namespace BerryTwinProducerConsumerModel
{
    public class Logger
    {
        private static readonly object LockObject = new object(); // Create a lock object to synchronize access to the console.
        private static readonly string LogFilePath = "log.txt"; // Define the path for the log file.

        public static void Log(string text, ConsoleColor color = ConsoleColor.White)
        {
            LogInternal(text, color); // Log a message with the specified text and color.
        }

        // Different methods for different levels of error logging

        public static void LogInfo(string text)
        {
            LogInternal($"INFO: {text}", ConsoleColor.Green); // Log an informational message with green text.
        }

        public static void LogWarning(string text)
        {
            LogInternal($"WARNING: {text}", ConsoleColor.Yellow); // Log a warning message with yellow text.
        }

        public static void LogError(string text, Exception exception = null)
        {
            LogInternal($"ERROR: {text}", ConsoleColor.Red); // Log an error message with red text.

            if (exception != null)
            {
                LogInternal($"Exception Details: {exception}", ConsoleColor.Red); // Log exception details if provided.
            }
        }

        private static void LogInternal(string text, ConsoleColor color)
        {
            lock (LockObject) // Synchronize access to the console to avoid conflicts.
            {
                Console.ForegroundColor = color; // Set the console text color.
                string logMessage = $"[{DateTime.UtcNow:hh:mm:ss.ff}] - {text}"; // Create a log message with a timestamp.
                Console.WriteLine(logMessage); // Display the log message on the console.
                WriteToFile(logMessage); // Write the log message to a log file.
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
