using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace HP4Mushrooms.HP4MBlockEntity
{
    [ProtoContract]
    public class MyceliumMushroomPacket
    {
        [ProtoMember(1)]
        public int SpawnMushroomId;
    }

    public enum EnumMyceliumPlantationPacketId
    {
        InfectedPlantation = 1000
    }

    public class BlockEntityMyceliumPlantation : Vintagestory.API.Common.BlockEntity
    {
        //private const int MAX_WAIT_SECONDS = 60 * 60 * 2;
        //private const int MIN_WAIT_SECONDS = 60 * 24;
        private const int MaxWaitSeconds = 10;
        private const int MinWaitSeconds = 1;

        private Guid _idBlock = Guid.NewGuid();
        private int _waitSeconds = 0;
        private int _currentWaitSeconds = 0;
        private int _spawnMushroomId = 0;

        private int WaitSeconds {
            get {
                if (_waitSeconds != 0) return _waitSeconds;
                var rnd = new Random();
                _waitSeconds = rnd.Next(MinWaitSeconds, MaxWaitSeconds);
                return _waitSeconds; 
            }
        }
        
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(OnTick, 1000);
        }

        private void OnTick(float par)
        {
            var block = Api.World.BlockAccessor.GetBlock(Pos);
            if (block.Code.Path.EndsWith("-base")) return;
                
            _currentWaitSeconds++;

            if ((_currentWaitSeconds <= WaitSeconds) || _spawnMushroomId == 0) return;
            
            _currentWaitSeconds = 0;
            _waitSeconds = 0;
            OnTimeGrow();
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("currentWaitSeconds", _currentWaitSeconds);
            tree.SetInt("waitSeconds", _waitSeconds);
            tree.SetInt("spawnMushroomId", _spawnMushroomId);
            tree.SetString("idBlock", _idBlock.ToString());
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            _currentWaitSeconds = tree.GetInt("currentWaitSeconds", 0);
            _waitSeconds = tree.GetInt("waitSeconds", 0);
            _spawnMushroomId = tree.GetInt("spawnMushroomId", 0);
            _idBlock = Guid.Parse(tree.GetString("idBlock", Guid.NewGuid().ToString()));
        }

        private void OnTimeGrow()
        {
            if (_spawnMushroomId == 0) return;
            
            var mushroomBlockType = Api.World.Blocks.FirstOrDefault(b => b.Id == _spawnMushroomId);
            if (mushroomBlockType is null)
            {
                Api.Logger.Warning($"Error get mushroom type by id: {_spawnMushroomId}, disable spawn");
                return;
            }
            
            if (mushroomBlockType.Code.Path.EndsWith("-normal"))
            {
                var upBlockPosition = Pos.UpCopy();
                var upBlock = Api.World.BlockAccessor.GetBlock(upBlockPosition);
                if (upBlock.Id != 0) return;
                Api.World.BlockAccessor.SetBlock(_spawnMushroomId, upBlockPosition);
            }
            else
            {
                var west = Pos.WestCopy();
                var north = Pos.NorthCopy();
                var south = Pos.SouthCopy();
                var east = Pos.EastCopy();

                var sourceBlock = Api.World.GetBlock(_spawnMushroomId);
                var codesMushroom = new List<(int, string)>()
                {
                    (Api.World.GetBlock(sourceBlock.CodeWithParts("east")).Id, "east"),
                    (Api.World.GetBlock(sourceBlock.CodeWithParts("south")).Id, "south"),
                    (Api.World.GetBlock(sourceBlock.CodeWithParts("north")).Id, "north"),
                    (Api.World.GetBlock(sourceBlock.CodeWithParts("west")).Id, "west"),
                };

                var positions = new List<(int, BlockPos, string)>()
                {
                    (Api.World.BlockAccessor.GetBlock(west).Id, west, "east"),
                    (Api.World.BlockAccessor.GetBlock(north).Id, north, "south"),
                    (Api.World.BlockAccessor.GetBlock(south).Id, south, "north"), 
                    (Api.World.BlockAccessor.GetBlock(east).Id, east, "west")
                };

                if (positions.All(b => !codesMushroom.Select(w => w.Item1).Contains(b.Item1)) && positions.Any(b => b.Item1 == 0))
                {
                    var firstFace = positions.First(b => b.Item1 == 0);
                    var needMushroom = codesMushroom.First(b => b.Item2 == firstFace.Item3);
                    Api.World.BlockAccessor.SetBlock(needMushroom.Item1, firstFace.Item2);
                }
            }
        }

        public void OnRightClick(IPlayer byPlayer)
        {
            var activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot?.Itemstack?.Block?.Code?.Path?.Contains("mushroom") != true) return;
            var mushroomId = activeSlot?.Itemstack?.Block.Id ?? 0;

            var packet = SerializerUtil.Serialize(new MyceliumMushroomPacket()
            {
                SpawnMushroomId = mushroomId
            });

            if(Api is ICoreClientAPI cap)
            {
                cap.Network.SendBlockEntityPacket(Pos, (int)EnumMyceliumPlantationPacketId.InfectedPlantation, packet);
            }
        }

        public override void OnReceivedClientPacket(IPlayer player, int packetid, byte[] data)
        {
            if (!Api.World.Claims.TryAccess(player, Pos, EnumBlockAccessFlags.BuildOrBreak))
            {
                player.InventoryManager.ActiveHotbarSlot.MarkDirty();
                return;
            }
            
            if (packetid == (int)EnumMyceliumPlantationPacketId.InfectedPlantation)
            {
                Api.Logger.Notification("Infected");
                var packet = SerializerUtil.Deserialize<MyceliumMushroomPacket>(data);
                var activeSlot = player.InventoryManager.ActiveHotbarSlot;
                
                Block block = Api.World.BlockAccessor.GetBlock(Pos);
                if (block.Code.Path.EndsWith("-base"))
                {
                    block = Api.World.GetBlock(block.CodeWithParts("infected"));
                    Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
                    Api.Logger.Notification("Infected Complete");
                }

                Api.Logger.Notification("SetupMushroom");
                var blockEntity = Api.World.BlockAccessor.GetBlockEntity<BlockEntityMyceliumPlantation>(Pos);
                blockEntity._spawnMushroomId = packet.SpawnMushroomId;
                
                activeSlot.TakeOut(1);
                activeSlot.MarkDirty();
                MarkDirty(true);
                
                Api.World.BlockAccessor.GetChunkAtBlockPos(Pos).MarkModified();
                Api.Logger.Notification("Complete SetupMushroom");
            }
        }
    }
}
