using Content.Client._ROBUST.Match.Kits;
using Content.Client._ROBUST.Match.Timer;
using Content.Shared._ROBUST.Match;
using Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;
using Robust.Shared.Network;

namespace Content.Client._ROBUST.Match.RobustLoadouts;

// TODO: Use UI controller?
public sealed partial class RobustLoadoutController : EntitySystem
{
    [Dependency] private IClientNetManager _net = default!;

    private RobustLoadoutSelectionWindow? _window;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RobustKitsMessage>(OnKitToChooseFrom);
    }

    private void OnKitToChooseFrom(RobustKitsMessage msg, EntitySessionEventArgs args)
    {
        // If a window is already open, close it
        _window?.Close();

        _window = new RobustLoadoutSelectionWindow(msg.Kits);
        _window.CountdownTimer.TimeFormat = TimeFormat.SecondsMilliseconds;
        _window.CountdownTimer.StartCountdown(msg.MaximumSelectionTime);
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
        _window.OnKitChosen += OnKitSelected;
        _window.CountdownTimer.OnTimerFinished += () => _window?.Visible = false; // todo: change this, close doesn't work because your closing in an update loop
    }

    private void OnKitSelected(int args)
    {
        var kit = new RobustKitsChosenMessage();
        kit.Kit = args;
        _net.ClientSendMessage(kit);
        _window?.Close();
    }
}
