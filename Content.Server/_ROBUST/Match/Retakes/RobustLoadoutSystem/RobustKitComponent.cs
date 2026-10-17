using Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._ROBUST.Match.Retakes.RobustLoadoutSystem;

[RegisterComponent]
public sealed partial class RobustKitComponent : Component
{
    /// <summary>
    ///     Player session -> List of 3 kits, each of which has a dictionary of name -> list of entities in that section.
    /// </summary>
    [DataField]
    public Dictionary<ICommonSession, List<ProtoId<RobustKitPrototype>>> AvailableKits = new();

    [DataField]
    public Dictionary<ICommonSession, int> ChosenKits = new();

    [DataField]
    public TimeSpan MaximumSelectionTime;

    [DataField]
    public TimeSpan? StopWaitingKitTime;
}
