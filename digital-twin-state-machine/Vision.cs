using System;
using System.IO;
using System.Threading;

public class Vision
{
    public static void Run(StreamWriter logFile, Random random, double visionErrorProbability)
    {
        int visionTime = 1000; // Run code like this to make the time static and adjust
        //int visionTime = random.Next(1000, 3000);
        ManufacturingDigitalTwin.LogAndOutput("Vision inspection is being performed...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= visionErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Vision inspection malfunctioned.", logFile);
            throw new Exception("Vision inspection malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(visionTime);
        }
    }
}