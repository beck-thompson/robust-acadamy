namespace Content.Server._ROBUST.Match.Retakes.Phases;


public sealed partial class RemoveTemporaryWalls : IPhase
{
    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var walls = entMan.EntityQueryEnumerator<TemporaryWallComponent>();

        foreach (var wall in walls)
        {
            entMan.DeleteEntity(wall);
        }

        EndPhase(match, entMan);
    }
}
