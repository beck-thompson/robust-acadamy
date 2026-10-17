using System.Linq;
using Content.Server._ROBUST.Match;
using Content.Shared._ROBUST.Match;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server._ROBUST.Match.Retakes;

public sealed partial class RetakesTeamSelection : IPhase
{
    private MatchManagerSystem _match = default!;

    [DataField(required: true)]
    public Dictionary<string, int> Teams;

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        // IoCManager.InjectDependencies(this);

        _match = entMan.System<MatchManagerSystem>();

        if (Teams.Count == 0)
            throw new Exception("There has to be at least one team");

        var players = _match.GetPlayers(match).Shuffle().ToList();

        var teamComp = entMan.EnsureComponent<MatchTeamComponent>(match);

        // todo: combine this with the other for loop
        foreach (var team in Teams)
        {
            teamComp.Teams.Add(team.Key, new List<ICommonSession>());
        }

        // Only two teams:

        List<ICommonSession> NukieTeam = [];
        List<ICommonSession> CrewTeam = [];

        if (players.Count == 1)
        {
            NukieTeam = [players.Pop()];
        }
        else if (players.Count == 2)
        {
            NukieTeam = [players.Pop()];
            CrewTeam = [players.Pop()];
        }
        else if (players.Count == 3)
        {
            NukieTeam = [players.Pop()];
            CrewTeam = [players.Pop(), players.Pop()];
        }
        else if (players.Count == 4)
        {
            NukieTeam = [players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 5)
        {
            NukieTeam = [players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 6)
        {
            NukieTeam = [players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 7)
        {
            NukieTeam = [players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 8)
        {
            NukieTeam = [players.Pop(), players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 9)
        {
            NukieTeam = [players.Pop(), players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop()];
        }
        else if (players.Count == 10)
        {
            NukieTeam = [players.Pop(), players.Pop(), players.Pop()];
            CrewTeam = [players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop(), players.Pop()];
        }

        teamComp.Teams["NukieTeam"] = NukieTeam;
        teamComp.Teams["CrewTeam"] = CrewTeam;

        EndPhase(match, entMan);
    }
}
