using Content.Server.Spawners.Components;
using Content.Shared._ROBUST.Match;
using Robust.Shared.Prototypes;

namespace Content.Server._ROBUST.Match.Spawners;

[RegisterComponent]
public sealed partial class MatchTeamSpawnPointComponent : Component
{
    [DataField]
    public ProtoId<TeamPrototype> ValidTeam;
}
