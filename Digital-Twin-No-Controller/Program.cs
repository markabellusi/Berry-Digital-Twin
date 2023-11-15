namespace BerryTwinProducerConsumerModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Channels;

    class Program
    {
        private static CancellationTokenSource tokenSource;
        static async Task Main(string[] args)
        {
            await RunHopperBlenderAndExtruder();

            Logger.Log("done!");
            Console.ReadLine();
        }

        private static async Task HandleErrorsAsync(Func<Task> taskFunction, string componentName)
        {
            try
            {
                await taskFunction.Invoke();
            }
            catch (OperationCanceledException)
            {
                Logger.Log($"{componentName} was interrupted.", ConsoleColor.Magenta);
            }
            catch (Exception ex)
            {
                Logger.LogError($"{componentName} error: {ex.Message}", ex);
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

                await Task.WhenAll(tasks);

                Logger.Log("*** EXECUTION COMPLETE ***");
            }

            private static async Task HopperProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                try
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
                    }

                catch (OperationCanceledException)
                {
                    // Handle operation cancellation.
                    Logger.Log("Hopper was interrupted.", ConsoleColor.Magenta);
                }
                catch (Exception ex)
                {
                    // Handle the error when Blender produces too much.
                    Logger.LogError($"Hopper error: {ex.Message}");
                }

                // Log that Hopper is done producing.
                Logger.Log("Hopper is done producing.", ConsoleColor.Blue);
                
            }
            
            private static async Task BlenderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                try
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

                }
                catch (OperationCanceledException)
                {
                    // Handle operation cancellation.
                    Logger.Log("Blender was interrupted.", ConsoleColor.Magenta);
                }
                catch (Exception ex)
                {
                    // Handle the error when Blender produces too much.
                    Logger.Log($"Blender error: {ex.Message}", ConsoleColor.Red);
                }

                Logger.Log("Blender is done producing.", ConsoleColor.Blue);
            }

          private static async Task BlenderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
         {
            // Create a Blender consumer instance.
            var blender = new BlenderConsumer(channel.Reader, "Blender");
            bool errorOccurred = false;
            int consumedCount = 0;
            int maxConsumedCount = 5; // Change this to control how much the Blender consumes

            try
            {
                await foreach (var message in blender.ConsumeAsync(cancellationToken))
                {
                    // Log the received message and simulate processing.
                    Logger.Log($"Blender received: {message.LogFile}");
                    await Task.Delay(500, cancellationToken);

                    // Simulate an error condition.
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
                // Handle operation cancellation.
                Logger.Log("Blender was interrupted.", ConsoleColor.Magenta);
            }
            catch (InvalidOperationException ex)
            {
                // Handle the simulated error in Blender.
                Logger.Log($"Blender error: {ex.Message}", ConsoleColor.Red);
                errorOccurred = true;
            }

            // Log that Blender is done consuming.
            Logger.Log("Blender is done consuming.", ConsoleColor.Blue);

            // If an error occurred in Blender, set the cancellation token to stop the other producers.
            if (errorOccurred)
            {
                tokenSource.Cancel();
            }
        }

            private static async Task ExtruderProducerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
            {
                try
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
                }

                catch (OperationCanceledException)
                {
                    // Handle operation cancellation.
                    Logger.Log("Extruder was interrupted.", ConsoleColor.Magenta);
                }
                catch (InvalidOperationException ex)
                {
                    // Handle the error when Extruder produces too much.
                    Logger.Log($"Extruder error: {ex.Message}", ConsoleColor.Red);
                }

                // Log that Extruder is done producing.
                Logger.Log("Extruder is done producing.", ConsoleColor.Magenta);
            }

            private static async Task ExtruderConsumerAsync(Channel<Envelope> channel, CancellationToken cancellationToken)
        {
            // Create an Extruder consumer instance.
            var extruder = new ExtruderConsumer(channel.Reader, "Extruder");
            bool errorOccurred = false;
            int consumedCount = 0;
            int maxConsumedCount = 10; // Change this to control how much the Extruder consumes

            try
            {
                await foreach (var message in extruder.ConsumeAsync(cancellationToken))
                {
                    // Log the received message and simulate processing.
                    Logger.Log($"Extruder received: {message.LogFile}");
                    await Task.Delay(500, cancellationToken);

                    // Simulate an error condition.
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
                // Handle operation cancellation.
                Logger.Log("Extruder was interrupted.", ConsoleColor.Magenta);
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
            if (errorOccurred)
            {
                tokenSource.Cancel();
            }
        }

        }


    }