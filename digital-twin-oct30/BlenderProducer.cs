using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{ 
    public class BlenderProducer
        {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;
            private int totalStates; // Total number of states for calculating completion.
            private int completedStates; // Number of states completed.

            public BlenderProducer(ChannelReader<Envelope> reader, ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
                completedStates = 0; // Initialize completed states.
                totalStates = Enum.GetValues(typeof(BlenderState)).Length;
            }

        public async Task ProduceBlenderStateAsync(BlenderState blenderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(BlenderState), blenderState));
                // Produce the Blender state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                // Caculate the compleation percentage and log it 
                completedStates++;

                double completionPercentage = (completedStates / (double)totalStates) * 100;
                Logger.Log($"{_name} > Produced blender state: '{Enum.GetName(typeof(BlenderState), blenderState)}', Completion: {completionPercentage:F2}%", ConsoleColor.Cyan);
            }
        }

        public enum BlenderState
        {
            State_B1,
            State_B2,
            State_B3,
            State_B4,
            State_B5,
            productFinished
        }
}
