using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class HopperProducer
    {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public HopperProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task PublishHopperStateAsync(HopperState hopperState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(HopperState), hopperState));
                // Publish the Hopper state message to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Published hopper state: '{Enum.GetName(typeof(HopperState), hopperState)}'", ConsoleColor.Yellow);
            }
    }

    public enum HopperState
    {
            DispensingBluePellets,
            InitializingSelfCheck,
            ProcessingData,
            PerformingCalibration,
            MonitoringEnvironment,
            EngagingSensors,
            AnalyzingDataStreams,
            GeneratingReports,
            PreparingForNextTask,
            IdleState
    }
}
