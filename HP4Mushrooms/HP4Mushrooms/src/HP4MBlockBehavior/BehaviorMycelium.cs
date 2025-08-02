using HP4Mushrooms.HP4MBlockEntity;
using Vintagestory.API.Common;

namespace HP4Mushrooms.HP4MBlockBehavior;

public class BehaviorMycelium : BlockBehavior
{
    public BehaviorMycelium(Block block) : base(block) { }
    
    public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
    {
        var entity = world.BlockAccessor.GetBlockEntity(blockSel.Position);
    
        if (entity is not BlockEntityMyceliumPlantation besiege) return true;
        besiege.OnRightClick(byPlayer);
            
        return false;
    }
}