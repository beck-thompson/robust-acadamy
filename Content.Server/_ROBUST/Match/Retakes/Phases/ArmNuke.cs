using Content.Server.Chat.Systems;
using Content.Server.Nuke;
using Content.Server.Pinpointer;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.Match.Retakes.Phases;

public sealed partial class ArmNuke : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var nukeSystem = entMan.System<NukeSystem>();
        var matchManager = entMan.System<MatchManagerSystem>();
        var navSystem = entMan.System<NavMapSystem>();
        var chatSystem = entMan.System<ChatSystem>();

        var nukes = entMan.EntityQueryEnumerator<NukeComponent>();

        var numNukes = 0;
        var matchMap = matchManager.GetMap(match);

        foreach (var nuke in nukes)
        {
            var nukeMapUid = entMan.GetComponent<TransformComponent>(nuke).MapUid;

            if (nukeMapUid != matchMap)
                continue;

            nukeSystem.ArmBomb(nuke);

            // you have to do this manually because there is no station
            // warn a crew
            var announcement = Loc.GetString("nuke-component-announcement-armed",
                ("time", (int) nuke.Comp.RemainingTime),
                ("location", FormattedMessage.RemoveMarkupOrThrow(navSystem.GetNearestBeaconString((nuke, null)))));
            var sender = Loc.GetString("nuke-component-announcement-sender");

            var filter = Filter.Empty();
            filter.AddPlayers(matchManager.GetPlayers(match));
            chatSystem.DispatchFilteredAnnouncement(filter, announcement, nuke, sender, false, null, Color.Red);

            numNukes++;
        }

        if (numNukes != 1)
            throw new Exception("More than one nuke!");

        EndPhase(match, entMan);
    }
}
