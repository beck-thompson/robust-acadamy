using Content.Shared._ROBUST.Match;
using Robust.Shared.Prototypes;

namespace Content.Server._ROBUST.Match.Spawners;

[RegisterComponent]
public sealed partial class MatchSpawnPointComponent : Component
{
    [DataField]
    public ProtoId<TeamPrototype>? ForceTeam;
}
