using System.Linq;
using Content.Server._ROBUST.DebugSystem;
using Content.Server._ROBUST.Match.Spawners;
using Content.Server.Atmos.EntitySystems;
using Content.Shared._ROBUST.Match;
using Content.Shared._ROBUST.Match.Retakes.RobustLoadouts;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._ROBUST.Match.Retakes.RobustLoadoutSystem;

public sealed partial class RobustLoadoutSystem : EntitySystem
{
    [Dependency] private MapLoaderSystem _loader = default!;
    [Dependency] private MetaDataSystem _metaDataSystem = default!;
    [Dependency] private SharedMapSystem _maps = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private AtmosphereSystem _atmosphere = default!;
    [Dependency] private ISharedPlayerManager _player = default!;
    [Dependency] private MatchManagerSystem _match = default!;
    [Dependency] private EntityTableSystem _table = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        _net.RegisterNetMessage<RobustKitsChosenMessage>(ReceiveKitChosen);
    }

    private void ReceiveKitChosen(RobustKitsChosenMessage message)
    {
        var player = _player.GetSessionByChannel(message.MsgChannel);

        var query = EntityQueryEnumerator<MatchKitComponent, MatchComponent>();
        while (query.MoveNext(out var uid, out var kitComp, out var matchComp))
        {
            if (!_match.GetPlayers(uid).Contains(player))
                continue;

            kitComp.ChosenKits.Add(player, message.Kit);

            if (kitComp.ChosenKits.Count == _match.GetPlayers(uid).Count)
            {
                _match.EndCurrentPhase(uid);

                // todo: do some kind of "stop looking for kits" method so we don't always need to remember to switch this off.
                kitComp.StopWaitingKitTime = null;
            }
        }
    }

    public EntProtoId GetPlayersLoadout(EntityUid match, ICommonSession player)
    {
        var loadoutComp = Comp<RobustKitComponent>(match);

        var chosenKitIndex = loadoutComp.ChosenKits[player];

        return ProtoMan.Index(loadoutComp.AvailableKits[player][chosenKitIndex]).Kit;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Get the current server time.
        var curTime = _timing.CurTime;

        var query = EntityQueryEnumerator<RobustKitComponent>();

        // Loop over all entities.
        while (query.MoveNext(out var uid, out var comp))
        {
            // Skip this entity if it should not be updated yet.
            if (comp.StopWaitingKitTime == null || comp.StopWaitingKitTime > curTime)
                continue;

            var players = _match.GetPlayers(uid);

            comp.StopWaitingKitTime = null;

            // This means the players already choose, don't do anything.
            if (comp.ChosenKits.Count == players.Count)
                continue;

            // todo: do check for if there are more kits than players !
            // choose kits at random for players who don't choose

            foreach (var player in players)
            {
                var playerData = player;
                if (comp.ChosenKits.ContainsKey(playerData))
                    continue;

                comp.ChosenKits.Add(playerData, _random.Next(comp.AvailableKits[playerData].Count));
            }

            _match.EndCurrentPhase(uid);
        }
    }
}

// list of rolls
// pick and take 5
// 1.) Choose roll
// 2.) Give kits from that roll

public sealed partial class RobustLoadoutAskForKits : IPhase
{
    // team -> roles
    //

    // todo:
    // List of roles you always want
    // certain roles can only be had x times e.g max of 5 secoffs etc..

    [DataField]
    public Dictionary<ProtoId<TeamPrototype>, List<ProtoId<RobustRolePrototype>>> KitTables = new();

    [DataField]
    public int KitsToSelect = 1;

    [DataField]
    public TimeSpan MaximumSelectionTime = TimeSpan.FromSeconds(10);

    public override void RunPhase(EntityUid match, IEntityManager entMan)
    {
        var matchSystem = entMan.System<MatchManagerSystem>();
        var teamSystem = entMan.System<MatchTeamsSystem>();
        var tableSystem = entMan.System<EntityTableSystem>();
        var randomManager = IoCManager.Resolve<IRobustRandom>();
        var protoManager = IoCManager.Resolve<IPrototypeManager>();

        var kitComp = entMan.EnsureComponent<RobustKitComponent>(match);

        kitComp.StopWaitingKitTime = matchSystem._timing.CurTime + MaximumSelectionTime;
        kitComp.MaximumSelectionTime = MaximumSelectionTime;

        var players = matchSystem.GetPlayers(match);

        kitComp.AvailableKits = new();



        foreach (var player in players)
        {
            var playerData = player;

            kitComp.AvailableKits.Add(playerData, new());

            // get team of player
            // pick and take 1 role from the list
            // add the kits from that role up to the max number

            var team = teamSystem.GetTeamOfPlayer(match, player);

            var roleProto = protoManager.Index(randomManager.PickAndTake(KitTables[team]));

            var kits = new List<ProtoId<RobustKitPrototype>>(roleProto.Kits);

            for (var i = 0; i < KitsToSelect; i++)
            {
                var kit = randomManager.PickAndTake(kits);
                kitComp.AvailableKits[playerData].Add(kit);
            }
        }

        foreach (var playerToKits in kitComp.AvailableKits)
        {
            var player =  playerToKits.Key;
            var kits = playerToKits.Value.ToList();

            var message = new RobustKitsMessage
            {
                Kits = kits,
                MaximumSelectionTime = kitComp.MaximumSelectionTime,
            };

            // this is RaiseNetworkEvent
            entMan.EntityNetManager.SendSystemNetworkMessage(message, player.Channel);
        }

        // entMan.System<DebugMatch>().Serialize(match, "AfterAskPlayersKits");
    }
}

// public sealed partial class GiveEquipment : IPhase
// {
//     [Dependency] private IPrototypeManager _proto;
//
//     [DataField]
//     public ProtoId<EntityTablePrototype> DefaultKit = "DefaultOneVsOneEquipment";
//
//     public override void RunPhase(EntityUid match, IEntityManager entMan)
//     {
//         IoCManager.InjectDependencies(this);
//
//         var matchSystem = entMan.System<MatchManagerSystem>();
//         var handsSystem = entMan.System<HandsSystem>();
//         var tableSystem = entMan.System<EntityTableSystem>();
//
//
//         foreach (var player in matchSystem.GetPlayers(match))
//         {
//             if (player.AttachedEntity == null)
//                 throw new Exception("Tried to give kit to player with no body!"); // todo figure this out
//
//             var kit = GetKit(player, match, entMan);
//             var defaultKit = tableSystem.GetSpawns(_proto.Index(DefaultKit).Table).ToList();
//
//             defaultKit.AddRange(kit);
//
//             EquipKit(player.AttachedEntity.Value, defaultKit, entMan);
//         }
//
//         entMan.System<DebugMatch>().Serialize(match, "AfterEquip");
//
//         EndPhase(match, entMan);
//     }
//
//     private List<EntProtoId> GetKit(ICommonSession player, EntityUid match, IEntityManager entMan)
//     {
//         var matchKitComp = entMan.GetComponentOrNull<MatchKitComponent>(match);
//
//         if (matchKitComp == null)
//             return new List<EntProtoId>();
//
//         var chosenKitIndex = matchKitComp.ChosenKits[player];
//
//         var chosenKit = matchKitComp.AvailableKits[player][chosenKitIndex];
//
//          List<EntProtoId> kit = new();
//
//          foreach (var section in chosenKit)
//          {
//              foreach (var proto in section.Value)
//              {
//                  kit.Add(proto);
//              }
//          }
//
//          return kit;
//     }
//
//     private void EquipKit(EntityUid player, List<EntProtoId> kit, IEntityManager entMan)
//     {
//         var containerSystem = entMan.System<SharedContainerSystem>();
//         var inventorySystem = entMan.System<InventorySystem>();
//         var storageSystem = entMan.System<StorageSystem>();
//         var handSystem = entMan.System<HandsSystem>();
//
//         foreach (var itemProtoId in kit)
//         {
//             var item = entMan.SpawnAtPosition(itemProtoId, entMan.GetComponent<TransformComponent>(player).Coordinates);
//
//             var inserted = false;
//             var enumerator = new InventorySystem.InventorySlotEnumerator(entMan.GetComponent<InventoryComponent>(player), SlotFlags.All & ~SlotFlags.POCKET & ~SlotFlags.BELT);
//             while (enumerator.MoveNext(out var slotContainer))
//             {
//                 // you have to spawn the item first, otherwise you can't equip it. Why? Because its too far out of range yes this is crazy.
//
//                 if (!inventorySystem.CanEquip(player, item, slotContainer.ID, out _))
//                     continue;
//
//                 // inventorySystem.TryEquip(player, item, slotContainer.ID, force: true);
//                 inserted = inventorySystem.TryEquip(player, item, slotContainer.ID, force: true);
//             }
//
//             if (inserted)
//                 continue;
//
//             if (inventorySystem.CanEquip(player, item, "pocket1", out _)
//                 && inventorySystem.TryEquip(player, item, "pocket1"))
//                 continue;
//
//             if (inventorySystem.CanEquip(player, item, "pocket2", out _)
//                 && inventorySystem.TryEquip(player, item, "pocket2"))
//                 continue;
//
//             if (inventorySystem.TryGetSlotContainer(player, "back", out var backSlot, out _)
//                 && backSlot.ContainedEntity.HasValue
//                 && storageSystem.Insert(backSlot.ContainedEntity.Value, item, out _))
//                 continue;
//
//             handSystem.TryPickupAnyHand(player, item, false);
//         }
//     }
// }

