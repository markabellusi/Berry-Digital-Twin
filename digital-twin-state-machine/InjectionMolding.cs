using System;
using System.IO;
using System.Threading;

public class InjectionMolding
{
    public static void Run(StreamWriter logFile, Random random, double injectionMoldingErrorProbability)
    {
        int injectionTime = 1000; // Run code like this to make the time static and adjust 
        //int injectionTime = random.Next(2000, 5000);
        ManufacturingDigitalTwin.LogAndOutput("Injection Molding is creating parts...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= injectionMoldingErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Injection Molding malfunctioned.", logFile);
            throw new Exception("Injection Molding malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(injectionTime);
        }
    }
}
