using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{ public class HopperProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;
            private int totalStates; // Total number of states for calculating completion.
            private int completedStates; // Number of states completed.


            public HopperProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name; 
                completedStates = 0; // Initialize completed states.
                totalStates = Enum.GetValues(typeof(HopperState)).Length;
               
            }

            public async Task PublishHopperStateAsync(HopperState hopperState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(HopperState), hopperState));
                // Publish the Hopper state message to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                // Caculate the compleation percentage and log it 
                completedStates++;
                double completionPercentage = (completedStates / (double)totalStates) * 100;

                Logger.Log($"{_name} > Published hopper state: '{Enum.GetName(typeof(HopperState), hopperState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Yellow);
            }
        }

       

        public enum HopperState
        {
            State_H1,
            State_H2,
            State_H3,
            State_H4,
            State_H5,
            productFinished


        }
}
