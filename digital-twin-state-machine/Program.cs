using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

class ManufacturingDigitalTwin
{
    private static string logFilePath = "manufacturing_log.txt";
    private static int totalCaps = 5; // Adjust the number of caps as needed
    private static int totalRuns = 1; // Adjust the number of runs as needed
    private static Random random = new Random();

    private static double blenderErrorProbability = 0.01; // Adjust chance of error
    private static double injectionMoldingErrorProbability = 0.01; // Adjust chance of error
    private static double conveyor1ErrorProbability = 0.1; // Adjust chance of error
    private static double liningErrorProbability = 0.01; // Adjust chance of error
    private static double conveyor2ErrorProbability = 0.01; // Adjust chance of error
    private static double slitterErrorProbability = 0.01; // Adjust chance of error
    private static double conveyor3ErrorProbability = 0.01; // Adjust chance of error
    private static double visionErrorProbability = 0.01; // Adjust chance of error
    private static double conveyor4ErrorProbability = 0.01; // Adjust chance of error
    private static double packingErrorProbability = 0.01; // Adjust chance of error

    private static BlockingCollection<string> logMessages = new BlockingCollection<string>();
    private static int capsProduced = 0;
    private static List<Task> processTasks = new List<Task>();

    static async Task Main(string[] args)
    {
        using (StreamWriter logFile = new StreamWriter(logFilePath))
        {
            try
            {
                for (int run = 1; run <= totalRuns; run++)
                {
                    logFile.WriteLine($"Run {run}");

                    // Reset capsProduced for each run
                    capsProduced = 0;

                    // Start tasks for each manufacturing process
                    StartProcessTask(() => ContinuousRun(Blender.Run, logFile, random, blenderErrorProbability));
                    StartProcessTask(() => ContinuousRun(InjectionMolding.Run, logFile, random, injectionMoldingErrorProbability));
                    StartProcessTask(() => ContinuousRun(Conveyor1.Run, logFile, random, conveyor1ErrorProbability));
                    StartProcessTask(() => ContinuousRun(LiningMachine.Run, logFile, random, liningErrorProbability));
                    StartProcessTask(() => ContinuousRun(Conveyor2.Run, logFile, random, conveyor2ErrorProbability));
                    StartProcessTask(() => ContinuousRun(Slitter.Run, logFile, random, slitterErrorProbability));
                    StartProcessTask(() => ContinuousRun(Conveyor3.Run, logFile, random, conveyor3ErrorProbability));
                    StartProcessTask(() => ContinuousRun(Vision.Run, logFile, random, visionErrorProbability));
                    StartProcessTask(() => ContinuousRun(Conveyor4.Run, logFile, random, conveyor4ErrorProbability));
                    StartProcessTask(() => ContinuousRun(PackagingMachine.Run, logFile, random, packingErrorProbability));

                    // Wait for all tasks to complete
                    await Task.WhenAll(processTasks);

                    logFile.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Manufacturing process stopped due to an error: {ex.Message}");
            }
        }

        Console.WriteLine("Manufacturing process completed.");
    }

    public static void LogAndOutput(string message, StreamWriter logFile)
    {
        string logMessage = $"{DateTime.Now}: {message}";
        Console.WriteLine(logMessage);
        logFile.WriteLine(logMessage);
        logMessages.Add(logMessage); // Add the log message to the blocking collection
    }

    private static void StartProcessTask(Action action)
    {
        // Start a new task for the given action
        var task = Task.Run(() => action());
        processTasks.Add(task);
    }

    private static void ContinuousRun(Action<StreamWriter, Random, double> process, StreamWriter logFile, Random random, double errorProbability)
    {
        while (capsProduced < totalCaps)
        {
            try
            {
                process(logFile, random, errorProbability);
                Interlocked.Increment(ref capsProduced);
            }
            catch (Exception ex)
            {
                LogAndOutput($"Error in {process.Method.Name}: {ex.Message}", logFile);
                break; // Exit the loop on error
            }
        }
    }
}
