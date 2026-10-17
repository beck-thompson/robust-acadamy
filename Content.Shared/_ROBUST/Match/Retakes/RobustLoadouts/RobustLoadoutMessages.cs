using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;

[Serializable, NetSerializable]
public sealed class RobustKitsMessage : EntityEventArgs
{
    public List<ProtoId<RobustKitPrototype>> Kits = new();

    public TimeSpan MaximumSelectionTime;
}

public sealed class RobustKitsChosenMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.EntityEvent;

    public int Kit = 0;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Kit = buffer.ReadInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Kit);
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableUnordered;
}
