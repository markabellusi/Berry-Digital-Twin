using System;
using System.IO;
using System.Threading;

public class Conveyor2
{
    public static void Run(StreamWriter logFile, Random random, double conveyor2ErrorProbability)
    {
        int conveyor2Time = 1000; // Run code like this to make the time static and adjust 
        //int conveyor2Time = 1000;
        ManufacturingDigitalTwin.LogAndOutput("Conveyor is moving material...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= conveyor2ErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Conveyor 2 malfunctioned.", logFile);
            throw new Exception("Conveyor 2 malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(conveyor2Time);
        }
    }
}
