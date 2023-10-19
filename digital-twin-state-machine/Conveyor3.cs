using System;
using System.IO;
using System.Threading;

public class Conveyor3
{
    public static void Run(StreamWriter logFile, Random random, double conveyor3ErrorProbability)
    {
        int conveyor3Time = 1000; // Run code like this to make the time static and adjust 
        //int conveyor3Time = 1000;
        ManufacturingDigitalTwin.LogAndOutput("Conveyor is moving material...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= conveyor3ErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Conveyor 3 malfunctioned.", logFile);
            throw new Exception("Conveyor 3 malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(conveyor3Time);
        }
    }
}