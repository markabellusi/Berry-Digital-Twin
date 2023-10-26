using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class ExtruderProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;
            private int totalStates; // Total number of states for calculating completion.
            private int completedStates; // Number of states completed.


            public ExtruderProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
                completedStates = 0; // Initialize completed states.
                totalStates = Enum.GetValues(typeof(ExtruderState)).Length;
            }

            public async Task ProduceExtruderStateAsync(ExtruderState extruderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(ExtruderState), extruderState));
                // Produce the Extruder state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                // Caculate the compleation percentage and log it 
                completedStates++;
                double completionPercentage = (completedStates / (double)totalStates) * 100;

                Logger.Log($"{_name} > Produced extruder state: '{Enum.GetName(typeof(ExtruderState), extruderState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Magenta);
            }
        }

        public enum ExtruderState
        {
            State_E1,
            State_E2,
            State_E3,
            State_E4,
            State_E5,
            productFinished_produced_20_caps  
        }
}
