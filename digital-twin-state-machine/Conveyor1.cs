using System;
using System.IO;
using System.Threading;

public class Conveyor1
{
    public static void Run(StreamWriter logFile, Random random, double conveyor1ErrorProbability)
    {
        int conveyor1Time = 1000; // Run code like this to make the time static and adjust 
        //int conveyor1Time = 1000;
        ManufacturingDigitalTwin.LogAndOutput("Conveyor is moving material...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= conveyor1ErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Conveyor 1 malfunctioned.", logFile);
            throw new Exception("Conveyor 1 malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(conveyor1Time);
        }
    }
}
