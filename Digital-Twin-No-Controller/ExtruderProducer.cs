using System.Threading.Channels;

namespace BerryTwinProducerConsumerModel
{
    public class BlenderProducer
    {
            private readonly ChannelWriter<Envelope> _writer;
            private readonly string _name;

            public BlenderProducer(ChannelWriter<Envelope> writer, string name)
            {
                _writer = writer;
                _name = name;
            }

            public async Task ProduceBlenderStateAsync(BlenderState blenderState, CancellationToken cancellationToken = default)
            {
                var message = new Envelope(Enum.GetName(typeof(BlenderState), blenderState));
                // Produce the Blender state message and publish it to the channel.
                await _writer.WriteAsync(message, cancellationToken);
                Logger.Log($"{_name} > Produced blender state: '{Enum.GetName(typeof(BlenderState), blenderState)}'", ConsoleColor.Cyan);
            }
    }

    public enum BlenderState
    {
            MixingPlasticAndChemicals,
            AdjustingTemperature,
            AddingColoringAgents,
            MeltingPlasticMixture,
            QualityControlChecks,
            PouringPlasticMixture,
            CoolingDown,
            PackagingFinishedProduct,
            LabelingProducts,
            ReadyForShipment
    }
}
