

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Channels;
    using System.IO;

namespace BerryTwinProducerConsumerModel
{
    class Program
    {
        private static CancellationTokenSource tokenSource;

        static async Task Main(string[] args)
        {
            string logDirectory = "logs";
            EnsureLogDirectoryExists(logDirectory);

            await RunHopperBlenderAndExtruder();

            Logger.Log("Done!");
            Console.ReadLine();
        }

        private static void EnsureLogDirectoryExists(string logDirectory)
        {
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
              
        private static async Task RunHopperBlenderAndExtruder()
        {
            Logger.Log("*** STARTING EXECUTION ***");

            // Create a bounded channel with a buffer size of 10.
            var channel = Channel.CreateBounded<Envelope>(10);

            tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var tasks = new List<Task>
    {
        // Start the Hopper producer.
        HopperProducerAsync(channel, cancellationToken),
        // Start the Blender producer.
        BlenderProducerAsync(channel, cancellationToken),
        // Start the Blender consumer.
        BlenderConsumerAsync(channel, cancellationToken),
        // Start the Extruder producer.
        ExtruderProducerAsync(channel, cancellationToken),
        // Start the Extruder consumer.
        ExtruderConsumerAsync(channel, cancellationToken)
    };

            // Wait for all tasks to complete or until a total of 20 caps are produced.
            await Task.WhenAll(tasks);
            Logger.Log("*** EXECUTION COMPLETE ***");
        }

        private static async Task HopperProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            try
            {
                var hopper = new HopperProducer(channel.Writer, "Hopper");

                var hopperStates = Enum.GetValues(typeof(HopperState)).Cast<HopperState>().ToArray();
                int currentIndex = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await hopper.PublishHopperStateAsync(hopperStates[currentIndex], cancellationToken);
                    currentIndex = (currentIndex + 1) % hopperStates.Length;
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Hopper was interrupted.", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Hopper error: {ex.Message}", ex);
            }

            Logger.Log("Hopper is done producing.", ConsoleColor.Blue);
        }

        private static async Task BlenderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            try
            {
                var blender = new BlenderProducer(channel.Reader, channel.Writer, "Blender");

                var blenderStates = Enum.GetValues(typeof(BlenderState)).Cast<BlenderState>().ToArray();
                int currentIndex = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await blender.ProduceBlenderStateAsync(blenderStates[currentIndex], cancellationToken);
                    currentIndex = (currentIndex + 1) % blenderStates.Length;
                    await Task.Delay(800, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Blender was interrupted.", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Blender error: {ex.Message}", ex);
            }

            Logger.Log("Blender is done producing.", ConsoleColor.Blue);
        }

        private static async Task BlenderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            var blender = new BlenderConsumer(channel.Reader, "Blender");
            bool errorOccurred = false;
            int consumedCount = 0;
            int maxConsumedCount = 30; // Adjust the value to control how much the Blender/Extruder consumes before an error

            try
            {
                await foreach (var message in blender.ConsumeAsync(cancellationToken))
                {
                    Logger.Log($"Blender received: {message.LogFile}");
                    await Task.Delay(500, cancellationToken);

                    consumedCount++;
                    if (consumedCount > maxConsumedCount)
                    {
                        Logger.Log("Simulating an error in Blender.", ConsoleColor.Red);
                        throw new InvalidOperationException("Blender consumed too much.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Blender was interrupted.", ConsoleColor.Magenta);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log($"Blender error: {ex.Message}", ConsoleColor.Red);
                errorOccurred = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Blender error: {ex.Message}", ex);
                errorOccurred = true;
            }

            Logger.Log("Blender is done consuming.", ConsoleColor.Blue);

            if (errorOccurred)
            {
                tokenSource.Cancel();
            }
        }

        private static async Task ExtruderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            try
            {
                var extruder = new ExtruderProducer(channel.Writer, "Extruder");

                var extruderStates = Enum.GetValues(typeof(ExtruderState)).Cast<ExtruderState>().ToArray();
                int currentIndex = 0;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await extruder.ProduceExtruderStateAsync(extruderStates[currentIndex], cancellationToken);
                    currentIndex = (currentIndex + 1) % extruderStates.Length;
                    await Task.Delay(600, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Extruder was interrupted.", ConsoleColor.Magenta);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log($"Extruder error: {ex.Message}", ConsoleColor.Red);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Extruder error: {ex.Message}", ex);
            }

            Logger.Log("Extruder is done producing.", ConsoleColor.Magenta);
        }

        private static async Task ExtruderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            var extruder = new ExtruderConsumer(channel.Reader, "Extruder");
            bool errorOccurred = false;
            int consumedCount = 0;
            int maxConsumedCount = 20; // Adjust the value to control how much the Blender/Extruder consumes before an error

            try
            {
                await foreach (var message in extruder.ConsumeAsync(cancellationToken))
                {
                    Logger.Log($"Extruder received: {message.LogFile}");
                    await Task.Delay(500, cancellationToken);

                    consumedCount++;
                    if (consumedCount > maxConsumedCount)
                    {
                        Logger.Log("Simulating an error in Extruder.", ConsoleColor.Red);
                        throw new InvalidOperationException("Extruder consumed too much.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Log("Extruder was interrupted.", ConsoleColor.Magenta);
            }
            catch (InvalidOperationException ex)
            {
                Logger.Log($"Extruder error: {ex.Message}", ConsoleColor.Red);
                errorOccurred = true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Extruder error: {ex.Message}", ex);
                errorOccurred = true;
            }

            Logger.Log("Extruder is done consuming.", ConsoleColor.Blue);

            if (errorOccurred)
            {
                tokenSource.Cancel();
            }
        }
    }

    public class BlenderConsumer
    {
        private readonly ChannelReader<Envelope> _reader; // A ChannelReader for reading Envelope objects.
        private readonly string _name; // The name associated with this consumer.

        public BlenderConsumer(ChannelReader<Envelope> reader, string name)
        {
            _reader = reader; // Constructor that initializes the ChannelReader and the name.
            _name = name;
        }

        public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green); // Log a message indicating the start of consumption.

            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                yield return message; // Asynchronously enumerate through messages from the ChannelReader and yield them to the caller.
            }

            Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red); // Log a message indicating the end of consumption.
        }
    }

    public class BlenderProducer
    {
        private readonly ChannelWriter<Envelope> _writer;
        private readonly string _name;
        private int totalStates; // Total number of states for calculating completion.
        private int completedStates; // Number of states completed.
        private int runCount;
        private int totalCompletionCount;

        public BlenderProducer(ChannelReader<Envelope> reader, ChannelWriter<Envelope> writer, string name)
        {
            _writer = writer;
            _name = name;
            completedStates = 0; // Initialize completed states.
            totalStates = Enum.GetValues(typeof(BlenderState)).Length;
        }

        public async Task ProduceBlenderStateAsync(BlenderState blenderState, CancellationToken cancellationToken = default)
        {
            if (blenderState == BlenderState.ProductionComplete)
            {
                runCount++;
                totalCompletionCount++;
                Logger.Log($"Blender Run Count: {runCount}", ConsoleColor.DarkRed);
            }

            var message = new Envelope(Enum.GetName(typeof(BlenderState), blenderState));
            await _writer.WriteAsync(message, cancellationToken);

            // Calculate the completion percentage and reset to 0 when it reaches 100
            completedStates = (completedStates + 1) % totalStates;
            double completionPercentage = (completedStates / (double)totalStates) * 100;

            Logger.Log($"{_name} > Produced blender state: '{Enum.GetName(typeof(BlenderState), blenderState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Cyan);
        }
    }

    public class ExtruderConsumer
    {
        private readonly ChannelReader<Envelope> _reader; // A ChannelReader for reading Envelope objects.
        private readonly string _name; // The name associated with this consumer.

        public ExtruderConsumer(ChannelReader<Envelope> reader, string name)
        {
            _reader = reader; // Constructor that initializes the ChannelReader and the name.
            _name = name;
        }

        public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green); // Log a message indicating the start of consumption.

            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                yield return message; // Asynchronously enumerate through messages from the ChannelReader and yield them to the caller.
            }

            Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red); // Log a message indicating the end of consumption.
        }
    }

    public class ExtruderProducer
    {
        private readonly ChannelWriter<Envelope> _writer;
        private readonly string _name;
        private int totalStates; // Total number of states for calculating completion.
        private int completedStates; // Number of states completed.
        private int runCount;
        private int totalCompletionCount;


        public ExtruderProducer(ChannelWriter<Envelope> writer, string name)
        {
            _writer = writer;
            _name = name;
            completedStates = 0; // Initialize completed states.
            totalStates = Enum.GetValues(typeof(ExtruderState)).Length;
        }

        public async Task ProduceExtruderStateAsync(ExtruderState extruderState, CancellationToken cancellationToken = default)
        {
            if (extruderState == ExtruderState.ProductionComplete)
            {
                runCount++;
                totalCompletionCount += 20;
                Logger.Log($"Extruder Run Count: {runCount}", ConsoleColor.DarkRed);
                Logger.Log("");
                Logger.Log($" Total Plastic Cup Count: {totalCompletionCount}", ConsoleColor.DarkRed);
                Logger.Log("");
            }

            var message = new Envelope(Enum.GetName(typeof(ExtruderState), extruderState));
            await _writer.WriteAsync(message, cancellationToken);

            // Calculate the completion percentage and reset to 0 when it reaches 100
            completedStates = (completedStates + 1) % totalStates;
            double completionPercentage = (completedStates / (double)totalStates) * 100;

            Logger.Log($"{_name} > Produced extruder state: '{Enum.GetName(typeof(ExtruderState), extruderState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Magenta);
        }
    }

    public class HopperProducer
    {
        private readonly ChannelWriter<Envelope> _writer;
        private readonly string _name;
        private int totalStates; // Total number of states for calculating completion.
        private int completedStates; // Number of states completed.
        private int runCount;
        private int totalCompletionCount;


        public HopperProducer(ChannelWriter<Envelope> writer, string name)
        {
            _writer = writer;
            _name = name;
            completedStates = 0; // Initialize completed states.
            totalStates = Enum.GetValues(typeof(HopperState)).Length;

        }

        public async Task PublishHopperStateAsync(HopperState hopperState, CancellationToken cancellationToken = default)
        {
            if (hopperState == HopperState.ProductionComplete)
            {
                runCount++;
                totalCompletionCount++;
                Logger.Log($"Hopper Run Count: {runCount}", ConsoleColor.DarkRed);
            }

            var message = new Envelope(Enum.GetName(typeof(HopperState), hopperState));
            await _writer.WriteAsync(message, cancellationToken);

            // Calculate the completion percentage and reset to 0 when it reaches 100
            completedStates = (completedStates + 1) % totalStates;
            double completionPercentage = (completedStates / (double)totalStates) * 100;

            Logger.Log($"{_name} > Published hopper state: '{Enum.GetName(typeof(HopperState), hopperState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Yellow);
        }
    }

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

    public enum HopperState
    {
        State_H1,
        State_H2,
        State_H3,
        State_H4,
        State_H5,
        ProductionComplete
    }

    public enum ExtruderState
    {
        State_E1,
        State_E2,
        State_E3,
        State_E4,
        State_E5,
        ProductionComplete
    }

    public enum BlenderState
    {
        State_B1,
        State_B2,
        State_B3,
        State_B4,
        State_B5,
        ProductionComplete
    }

    public class Envelope
    {
        public Envelope(string logFile)
        {
            LogFile = logFile; // Constructor that initializes the LogFile property with the provided logFile parameter.
        }

        public string LogFile { get; } // Public read-only property that stores the log file path.
    }
}
