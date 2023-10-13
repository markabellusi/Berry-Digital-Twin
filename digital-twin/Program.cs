namespace BerryTwinProducerConsumerModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Channels;

    namespace BerryTwinProducerConsumerModel
    {
        class Program
        {
            static async Task Main(string[] args)
            {
                await RunHopperBlenderAndExtruder();

                Logger.Log("done!");
                Console.ReadLine();
            }

            private static async Task RunHopperBlenderAndExtruder()
            {
                Logger.Log("*** STARTING EXECUTION ***");

                // Create a bounded channel with a buffer size of 10.
                var channel = Channel.CreateBounded<Envelope>(10);

                var tokenSource = new CancellationTokenSource();
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

                await Task.WhenAll(tasks);

                Logger.Log("*** EXECUTION COMPLETE ***");
            }

            private static async Task HopperProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                // Create a Hopper producer instance.
                var hopper = new HopperProducer(channel.Writer, "Hopper");

                // Define possible Hopper states.
                var hopperStates = Enum.GetValues(typeof(HopperState)).Cast<HopperState>().ToList();

                Random random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Generate a random Hopper state.
                    var hopperState = hopperStates[random.Next(hopperStates.Count)];
                    // Publish the Hopper data to the channel.
                    await hopper.PublishHopperStateAsync(hopperState, cancellationToken);
                    // Simulate some work.
                    await Task.Delay(1000, cancellationToken);
                }

                // Log that Hopper is done producing.
                Logger.Log("Hopper is done producing.", ConsoleColor.Blue);
            }

            private static async Task BlenderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                // Create a Blender producer instance.
                var blender = new BlenderProducer(channel.Writer, "Blender");

                // Define possible Blender states.
                var blenderStates = Enum.GetValues(typeof(BlenderState)).Cast<BlenderState>().ToList();

                Random random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Generate a random Blender state.
                    var blenderState = blenderStates[random.Next(blenderStates.Count)];
                    // Produce the Blender state message and publish it to the channel.
                    await blender.ProduceBlenderStateAsync(blenderState, cancellationToken);
                    // Simulate some work.
                    await Task.Delay(800, cancellationToken);
                }

                Logger.Log("Blender is done producing.", ConsoleColor.Blue);
            }

            private static async Task BlenderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                // Create a Blender consumer instance.
                var blender = new BlenderConsumer(channel.Reader, "Blender");
                int consumedCount = 0;
                int maxConsumedCount = 20; // Maximum messages Blender can consume

                try
                {
                    await foreach (var message in blender.ConsumeAsync(cancellationToken))
                    {
                        consumedCount++;

                        if (consumedCount > maxConsumedCount)
                        {
                            // Simulate an error when Blender consumes too much.
                            Logger.Log("Blender is overflowing! Simulating an error.", ConsoleColor.Red);
                            throw new InvalidOperationException("Blender consumed too much.");
                        }

                        // Log the received message and simulate processing.
                        Logger.Log($"Blender received: {message.LogFile}");
                        await Task.Delay(500, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Handle operation cancellation.
                    Logger.Log("Blender was interrupted.", ConsoleColor.Magenta);
                }
                catch (InvalidOperationException ex)
                {
                    // Handle the error when Blender consumes too much.
                    Logger.Log($"Blender error: {ex.Message}", ConsoleColor.Red);
                }

                Logger.Log("Blender is done consuming.");
            }

            private static async Task ExtruderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                // Create an Extruder producer instance.
                var extruder = new ExtruderProducer(channel.Writer, "Extruder");

                // Define possible Extruder states.
                var extruderStates = Enum.GetValues(typeof(ExtruderState)).Cast<ExtruderState>().ToList();

                Random random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    // Generate a random Extruder state.
                    var extruderState = extruderStates[random.Next(extruderStates.Count)];
                    // Produce the Extruder state message and publish it to the channel.
                    await extruder.ProduceExtruderStateAsync(extruderState, cancellationToken);
                    // Simulate some work.
                    await Task.Delay(600, cancellationToken);
                }

                // Log that Extruder is done producing.
                Logger.Log("Extruder is done producing.", ConsoleColor.Magenta);
            }

            private static async Task ExtruderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                // Create an Extruder consumer instance.
                var extruder = new ExtruderConsumer(channel.Reader, "Extruder");
                bool errorOccurred = false;
                try
                {
                    await foreach (var message in extruder.ConsumeAsync(cancellationToken))
                    {
                        // Log the received message and simulate processing.
                        Logger.Log($"Extruder received: {message.LogFile}");
                        await Task.Delay(500, cancellationToken);

                        // Simulate an error condition.
                        if (message.LogFile.Contains("ErrorCondition"))
                        {
                            Logger.Log("Simulating an error in Extruder.", ConsoleColor.Red);
                            throw new InvalidOperationException("Extruder encountered an error.");
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    // Handle the simulated error in Extruder.
                    Logger.Log($"Extruder error: {ex.Message}", ConsoleColor.Red);
                    errorOccurred = true;
                }

                // Log that Extruder is done consuming.
                Logger.Log("Extruder is done consuming.", ConsoleColor.Blue);

                // If an error occurred in Extruder, set the cancellation token to stop the other producers.

                // cntrl + k + c  comments out
                // cntrl + k + u uncomments
                //if (errorOccurred)
                //{
                //    tokenSource.Cancel();
                //}
            }
        }

        public class HopperProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public HopperProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task PublishHopperStateAsync(HopperState hopperState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(HopperState), hopperState));
                // Publish the Hopper state message to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Published hopper state: '{Enum.GetName(typeof(HopperState), hopperState)}'", ConsoleColor.Yellow);
            }
        }

        public enum HopperState
        {
            DispensingBluePellets,
            InitializingSelfCheck,
            ProcessingData,
            PerformingCalibration,
            MonitoringEnvironment,
            EngagingSensors,
            AnalyzingDataStreams,
            GeneratingReports,
            PreparingForNextTask,
            IdleState
        }

        public class BlenderProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public BlenderProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task ProduceBlenderStateAsync(BlenderState blenderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(BlenderState), blenderState));
                // Produce the Blender state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Produced blender state: '{Enum.GetName(typeof(BlenderState), blenderState)}'", ConsoleColor.Cyan);
            }
        }

        public enum BlenderState
        {
            MixingPlasticAndChemicals,
            AdjustingTemperature,
            AddingColoringAgents,
            MeltingPlasticMixture,
            QualityControlChecks,
            PouringPlasticMixture,
            CoolingDown,
            PackagingFinishedProduct,
            LabelingProducts,
            ReadyForShipment
        }

        public class ExtruderProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public ExtruderProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task ProduceExtruderStateAsync(ExtruderState extruderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(ExtruderState), extruderState));
                // Produce the Extruder state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Produced extruder state: '{Enum.GetName(typeof(ExtruderState), extruderState)}'", ConsoleColor.Magenta);
            }
        }

        public enum ExtruderState
        {
            MixingPlasticAndChemicals,
            ShapingPlasticBottles,
            QualityControlChecks,
            PackagingPlasticBottles,
            LabelingProducts,
            ReadyForShipment
        }

        // Rest of the code (Logger, Envelope, etc.) remains the same.
    }

    public class BlenderConsumer
    {
        private readonly ChannelReader<Envelope> _reader;
        private readonly string _name;

        public BlenderConsumer(ChannelReader<Envelope> reader, string name)
        {
            _reader = reader;
            _name = name;
        }

        public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green);

            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                yield return message;
            }

            Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red);
        }
    }

    public class ExtruderConsumer
    { 
            private readonly ChannelReader<Envelope> _reader;
            private readonly string _name;

            public ExtruderConsumer(ChannelReader<Envelope> reader, string name)
            {
                _reader = reader;
                _name = name;

            }
            public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
            {
                Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green);

                await foreach (var message in _reader.ReadAllAsync(cancellationToken))
                {
                    yield return message;
                }

                Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red);
            }
    }

        public class Logger
        {
            private static readonly object LockObject = new object();
            private static readonly string LogFilePath = "log.txt";

            public static void Log(string text, ConsoleColor color = ConsoleColor.White)
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

    public class Envelope
    {
            public Envelope(string logFile)
            {
                LogFile = logFile;
            }

            public string LogFile { get; }
    }
}

