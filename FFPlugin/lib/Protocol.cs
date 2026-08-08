namespace FreezeFrame.Protocol
{
    public static class Consts {
        public const string PipeName = "DeweyDot.FreezeFrame";
        public const byte PayloadFlag = 0x80;
        public const byte OpcodeMask = 0x7F;
        public const int HeaderBytes = 4;
        public const int MaxPayloadLength = 0xFFFFFF;
    }
    public enum Opcode : byte
    {
        Disable = 0x00,
        Enable = 0x01,
        StepFixed = 0x02,
        StepUpdate = 0x03,
        LoadScene = 0x04,
    }
    public struct StepPayload
    {
        public float mouseX; // 0..3
        public float mouseY; // 4..7
        public byte buttonsBitmask1; // Forward, Left, Backward, Right, Dodge, Slide, Jump, Primary Fire
        public byte buttonsBitmask2; // Secondary Fire, Revolver, Shotgun, Nailgun, Railcannon, Rocket Launcher, Variation Slot 2, Variation Slot 3
        public byte buttonsBitmask3; // Change Fist, Feedbacker, Knuckleblaster, Hook
    }
}