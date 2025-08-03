using System;
using System.Net.NetworkInformation;
using HP4Mushrooms.HP4MBlockBehavior;
using HP4Mushrooms.HP4MBlockEntity;
using HP4Mushrooms.HP4ModConfig;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace HP4Mushrooms
{
    public class Hp4MushroomsModSystem : ModSystem
    {
        private ICoreServerAPI _serverApi;
        private IServerNetworkChannel _serverChannel;
        
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification($"Starting register BlockEntity mod {Mod.Info.ModID}: " + api.Side);
            api.RegisterBlockEntityClass("mycelium-plantation", typeof(BlockEntityMyceliumPlantation));
            Mod.Logger.Notification($"Complete register BlockEntity mod {Mod.Info.ModID}: " + api.Side);

            Mod.Logger.Notification($"Starting register BlockBehavior mod {Mod.Info.ModID}: " + api.Side);
            api.RegisterBlockBehaviorClass("behavior-mycelium-plantation", typeof(BehaviorMycelium));
            Mod.Logger.Notification($"Complete register BlockBehavior mod {Mod.Info.ModID}: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification($"Server side {Mod.Info.ModID}: " + Lang.Get("hp4mushrooms:startup"));
            _serverApi = api;
            
            _serverChannel = _serverApi.Network.RegisterChannel(Mod.Info.ModID)
                .RegisterMessageType<SyncClientConfigPacket>()
                .SetMessageHandler<SyncClientConfigPacket>((player, packet) => {});
            
            _serverApi.Event.PlayerJoin += OnPlayerJoin;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {

            Mod.Logger.Notification($"Client side {Mod.Info.ModID}: " + Lang.Get("hp4mushrooms:startup"));
            
            api.Network.RegisterChannel(Mod.Info.ModID)
                .RegisterMessageType<SyncClientConfigPacket>()
                .SetMessageHandler<SyncClientConfigPacket>(packet =>
                {
                    Hp4MModConfig.Loaded.MaxWaitSeconds = packet.MaxWaitSeconds;
                    Hp4MModConfig.Loaded.MinWaitSeconds = packet.MinWaitSeconds;
                });
        }

        public override void StartPre(ICoreAPI api)
        {
            var cfgFileName = $"{Mod.Info.ModID}.json";
            try
            {
                Hp4MModConfig fromDisk;
                if ((fromDisk = api.LoadModConfig<Hp4MModConfig>(cfgFileName)) == null)
                {
                    api.StoreModConfig(Hp4MModConfig.Loaded, cfgFileName);
                }
                else
                {
                    Hp4MModConfig.Loaded = fromDisk;
                }
            }
            catch
            {
                api.StoreModConfig(Hp4MModConfig.Loaded, cfgFileName);
            }
            
            api.Logger.Notification($"Read config {Mod.Info.ModID}: {Hp4MModConfig.Loaded.MaxWaitSeconds} {Hp4MModConfig.Loaded.MinWaitSeconds}");
            
            base.StartPre(api);
        }

        private void OnPlayerJoin(IServerPlayer player)
        {
            try
            {
                _serverChannel.SendPacket(new SyncClientConfigPacket
                {
                    MinWaitSeconds = Hp4MModConfig.Loaded.MinWaitSeconds,
                    MaxWaitSeconds = Hp4MModConfig.Loaded.MaxWaitSeconds
                }, player);
            }
            catch (Exception e)
            {
                _serverApi.Logger.Warning(new Exception($"Error send sync mod config settings to client: {player.ClientId}", e));
            }
        }
    }
}
