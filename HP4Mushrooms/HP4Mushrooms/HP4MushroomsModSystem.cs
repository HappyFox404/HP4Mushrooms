using HP4Mushrooms.HP4MBlockBehavior;
using HP4Mushrooms.HP4MBlockEntity;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace HP4Mushrooms
{
    public class Hp4MushroomsModSystem : ModSystem
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            Mod.Logger.Notification("Starting register BlockEntity mod hp4mushrooms: " + api.Side);
            api.RegisterBlockEntityClass("mycelium-plantation", typeof(BlockEntityMyceliumPlantation));
            Mod.Logger.Notification("Complete register BlockEntity mod hp4mushrooms: " + api.Side);

            Mod.Logger.Notification("Starting register BlockBehavior mod hp4mushrooms: " + api.Side);
            api.RegisterBlockBehaviorClass("behavior-mycelium-plantation", typeof(BehaviorMycelium));
            Mod.Logger.Notification("Complete register BlockBehavior mod hp4mushrooms: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Server side hp4mushrooms: " + Lang.Get("hp4mushrooms:startup"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {

            Mod.Logger.Notification("Client side hp4mushrooms: " + Lang.Get("hp4mushrooms:startup"));
        }
    }
}
