namespace BerryTwinProducerConsumerModel
{
    public class Envelope
    {
            public Envelope(string logFile)
            {
                LogFile = logFile;
            }

            public string LogFile { get; }   
    }
}
