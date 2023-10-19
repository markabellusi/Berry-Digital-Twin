using System;
using System.IO;
using System.Threading;

public class Conveyor4
{
    public static void Run(StreamWriter logFile, Random random, double conveyor4ErrorProbability)
    {
        int conveyor4Time = 1000; // Run code like this to make the time static and adjust 
        //int conveyor4Time = 1000;
        ManufacturingDigitalTwin.LogAndOutput("Conveyor is moving material...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= conveyor4ErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Conveyor 4 malfunctioned.", logFile);
            throw new Exception("Conveyor 4 malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(conveyor4Time);
        }
    }
}
