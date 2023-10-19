using System;
using System.IO;
using System.Threading;

public class LiningMachine
{
    public static void Run(StreamWriter logFile, Random random, double liningMachineErrorProbability)
    {
        int liningTime = 1000; // Run code like this to make the time static and adjust
        //int liningTime = random.Next(1000, 3000);
        ManufacturingDigitalTwin.LogAndOutput("Lining Machine is putting liners in parts...", logFile);

        double randomValue = random.NextDouble();

        if (randomValue <= liningMachineErrorProbability)
        {
            ManufacturingDigitalTwin.LogAndOutput("Error: Lining Machine malfunctioned.", logFile);
            throw new Exception("Lining Machine malfunctioned."); // Throw an exception to stop the process
        }
        else
        {
            Thread.Sleep(liningTime);
        }
    }
}