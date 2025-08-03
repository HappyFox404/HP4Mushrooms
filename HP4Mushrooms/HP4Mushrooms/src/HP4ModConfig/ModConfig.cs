using ProtoBuf;

namespace HP4Mushrooms.HP4ModConfig
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SyncClientConfigPacket
    {
        public int MaxWaitSeconds;
        public int MinWaitSeconds;
    }

    public class Hp4MModConfig
    {
        public static Hp4MModConfig Loaded { get; set; } = new Hp4MModConfig();
        
        /// <summary>
        /// Маскимальное время ожидания перед созданием граба на плантации. (Указывыется в сек) (По умолчанию сутки 48 реальных минут)
        /// </summary>
        public int MaxWaitSeconds { get; set; } = 10;
        /// <summary>
        /// Минимальное время ожидания перед созданием граба на плантации. (Указывыется в сек) (По умолчанию сутки 48 реальных минут)
        /// </summary>
        public int MinWaitSeconds { get; set; } = 1;
    }
}

