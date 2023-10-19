using System;
using System.IO;
using System.Threading;

public class Slitter
{
    public static void Run(StreamWriter logFile, Random random, double slitterErrorProbability)
    {
        int slitterTime = 1000; // Run code like this to make the time static and adjust
        //int slitterTime = random.Next(1000, 3000);
        ManufacturingDigitalTwin.LogAndOutput("Slitter is processing parts...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= slitterErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Slitter malfunctioned.", logFile);
            throw new Exception("Slitter malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(slitterTime);
        }
    }
}