using Robust.Shared.Prototypes;

namespace Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;

[Prototype]
public sealed partial class RobustKitPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required:true)]
    public string Name = "";

    [DataField(required:true)]
    public string Description = "";

    [DataField(required:true)]
    public EntProtoId Kit;
}
