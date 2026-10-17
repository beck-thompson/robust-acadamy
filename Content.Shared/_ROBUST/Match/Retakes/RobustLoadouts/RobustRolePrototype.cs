using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;

[Prototype]
public sealed partial class RobustRolePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public List<ProtoId<RobustKitPrototype>> Kits = new();
}
