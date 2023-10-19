using System;
using System.IO;
using System.Threading;

public class Blender
{
    public static void Run(StreamWriter logFile, Random random, double blenderErrorProbability)
    {
        int blendingTime = 1000; // Run code like this to make the time static and adjust 
        // int blendingTime = random.Next(1000, 3000);
        ManufacturingDigitalTwin.LogAndOutput("Blender is creating material...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= blenderErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Blender malfunctioned.", logFile);
            throw new Exception("Blender malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(blendingTime);
        }
    }
}