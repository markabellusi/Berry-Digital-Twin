namespace BerryTwinProducerConsumerModel
{
    public class Envelope
    {
        public Envelope(string logFile)
        {
            LogFile = logFile; // Constructor that initializes the LogFile property with the provided logFile parameter.
        }

        public string LogFile { get; } // Public read-only property that stores the log file path.
    }
}
