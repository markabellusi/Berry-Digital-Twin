using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class ExtruderProducer
    {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public ExtruderProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task ProduceExtruderStateAsync(ExtruderState extruderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(ExtruderState), extruderState));
                // Produce the Extruder state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Produced extruder state: '{Enum.GetName(typeof(ExtruderState), extruderState)}'", ConsoleColor.Magenta);
            }
    }

    public enum ExtruderState
    {
            MixingPlasticAndChemicals,
            ShapingPlasticBottles,
            QualityControlChecks,
            PackagingPlasticBottles,
            LabelingProducts,
            ReadyForShipment
    }
}
