using System;
using System.IO;
using System.Threading;

public class PackagingMachine
{
    public static void Run(StreamWriter logFile, Random random, double packingErrorProbability)
    {
        int packagingTime = 1000; // Run code like this to make the time static and adjust
        //int packagingTime = random.Next(1000, 3000);
        ManufacturingDigitalTwin.LogAndOutput("Packaging Machine is filling boxes...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= packingErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Packaging Machine malfunctioned.", logFile);
            throw new Exception("Packaging Machine malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(packagingTime);
        }
    }
}