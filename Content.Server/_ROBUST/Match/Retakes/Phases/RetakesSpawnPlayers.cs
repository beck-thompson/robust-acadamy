using System.Linq;
using Content.Server._ROBUST.Match.Spawners;
using Content.Shared._ROBUST.Match;
using Robust.Server.Player;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;
using Content.Server._ROBUST.Match.Retakes.RobustLoadoutSystem;

namespace Content.Server._ROBUST.Match.Retakes.Phases;

public sealed partial class RetakesSpawnPlayers : IPhase
{
    private MatchTeamsSystem _team = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    private MetaDataSystem _meta = default!;
    private MatchManagerSystem _match = default!;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        // spawn players w/ the entities that they chose


        IoCManager.InjectDependencies(this);
        _team = entMan.System<MatchTeamsSystem>();
        // _player = entMan.EntitySysManager<IPlayerManager>();
        _meta = entMan.System<MetaDataSystem>();
        _match = entMan.System<MatchManagerSystem>();
        _team = entMan.System<MatchTeamsSystem>();

        var targetGrid = _match.GetGrid(match);

        // todo: figure this sus thing out - not used out var etc...
        if (!entMan.TryGetComponent<MapGridComponent>(targetGrid, out var comp))
            return;

        var allSpawnPoints = entMan.EntityQueryEnumerator<MatchSpawnPointComponent>();
        var spawns = new List<Entity<MatchSpawnPointComponent>>();

        while (allSpawnPoints.MoveNext(out var uid, out var spawnComp))
        {
            if (entMan.GetComponent<TransformComponent>(uid).GridUid != targetGrid)
                continue;

            spawns.Add((uid, spawnComp));
        }

        if (spawns.Count < _match.GetPlayers(match).Count)
        {
            throw new Exception($"INVALID NUMBER PLAYERS");
        }

        spawns = spawns.Shuffle().ToList();

        List<EntityUid> oldBodies = [];

        foreach (var player in _match.GetPlayers(match))
        {
            if (player.AttachedEntity != null)
                oldBodies.Add(player.AttachedEntity.Value);
        }

        foreach (var playersInTeam in _team.GetTeamsAndTheirPlayers(match))
        {

            var playersTeam = _team.GetTeamOfPlayer(match, playersInTeam[0]);

            // find valid team
            EntityUid teamSpawn = new();
            var found = false;

            foreach (var spawn in spawns)
            {
                if (spawn.Comp.ForceTeam == null)
                    throw new Exception("Spawn force team is null but logic can't handle that");

                if (spawn.Comp.ForceTeam.ToString() == playersTeam)
                {
                    teamSpawn = spawn;
                    found = true;
                }
            }

            if (!found)
                throw new Exception("Couldn't find team!");

            var teamSpawnLocation = entMan.GetComponent<TransformComponent>(teamSpawn).Coordinates;

            foreach (var player in playersInTeam)
            {
                // todo: why does it look like this???
                var chosenKit = entMan.System<RobustLoadoutSystem.RobustLoadoutSystem>().GetPlayersLoadout(match, player);

                var urst = entMan.SpawnAtPosition(chosenKit, teamSpawnLocation);

                _meta.SetEntityName(urst, player.Name);

                var team = _team.GetTeamOfPlayer(match, player);

                var teamComp = entMan.EnsureComponent<TeamComponent>(urst);
                teamComp.CurrentTeam = team;
                entMan.Dirty(urst, teamComp);

                _player.SetAttachedEntity(player, urst, true);
            }
        }

        // Delete old player bodies
        // todo actually delete not move to nullspace - delete doesn't work for some reason...
        foreach (var body in oldBodies)
        {
            // _transform.DetachEntity(body);
            // QueueDel(body);

            entMan.DeleteEntity(body);
        }

        EndPhase(match, entMan);

    }
}
