using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class ExtruderConsumer
    {
        private readonly ChannelReader<Envelope> _reader; // A ChannelReader for reading Envelope objects.
        private readonly string _name; // The name associated with this consumer.

        public ExtruderConsumer(ChannelReader<Envelope> reader, string name)
        {
            _reader = reader; // Constructor that initializes the ChannelReader and the name.
            _name = name;
        }

        public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green); // Log a message indicating the start of consumption.

            await foreach (var message in _reader.ReadAllAsync(cancellationToken))
            {
                yield return message; // Asynchronously enumerate through messages from the ChannelReader and yield them to the caller.
            }

            Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red); // Log a message indicating the end of consumption.
        }
    }
}
