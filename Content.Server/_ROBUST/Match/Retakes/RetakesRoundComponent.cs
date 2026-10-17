namespace Content.Server._ROBUST.Match.Retakes;

[RegisterComponent]
public sealed partial class RetakesRoundComponent : Component
{
    // tood: do something less sus than this
    [DataField]
    public bool HasFinished;
}
