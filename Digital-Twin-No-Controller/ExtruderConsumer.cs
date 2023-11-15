using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class ExtruderConsumer
    { 
            private readonly ChannelReader<Envelope> _reader;
            private readonly string _name;

            public ExtruderConsumer(ChannelReader<Envelope> reader, string name)
            {
                _reader = reader;
                _name = name;

            }
            public async IAsyncEnumerable<Envelope> ConsumeAsync(CancellationToken cancellationToken = default)
            {
                Logger.Log($"{_name} > Starting to consume", ConsoleColor.Green);

                await foreach (var message in _reader.ReadAllAsync(cancellationToken))
                {
                    yield return message;
                }

                Logger.Log($"{_name} > Stopping consumption", ConsoleColor.Red);
            }
    }
}
