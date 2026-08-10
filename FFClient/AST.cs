namespace FFClient.AST
{
    abstract record Node
    {
        public required int Row { get; init; }
        public required int Col { get; init; }
        public required int Length { get; init; }
    };
    record LevelStart(LevelName level) : Node();
    record LevelName(string name) : Node();
    record Breakpoint() : Node();
    record Input(FrameCount Count, Aim Aim, List<Button> Buttons) : Node();
    record FrameCount(int Value) : Node();
    abstract record Aim() : Node();
    record AimDiscrete(AimVector Vec) : Aim();
    record AimContinuous(AimVector Vec1, AimVector Vec2) : Aim();
    record AimBlank() : Aim();
    abstract record AimVector() : Node();
    record AimVectorAngle(FloatValue X, FloatValue Y) : AimVector();
    record AimVectorPoint(FloatValue X, FloatValue Y, FloatValue Z) : AimVector();
    record FloatValue(float Value) : Node();
    abstract record Button(Key Key) : Node();
    record ButtonPress(Key Key) : Button(Key);
    record ButtonRelease(Key Key) : Button(Key);
    record ButtonHoldExplicit(Key Key, int Count) : Button(Key);
    record ButtonHoldImplicit(Key Key) : Button(Key);
    enum Key : ushort {
        Forward = 'W',
        Backward = 'S',
        Left = 'A',
        Right = 'D',
        Dodge = 'H',
        Slide = 'L',
        PrimaryFire = 'M',
        SecondaryFire = 'N',
        Revolver = 'Z',
        Shotgun = 'X',
        Nailgun = 'C',
        Railcannon = 'V',
        RocketLauncher = 'B',
        VariationSlot2 = 'E',
        VariationSlot3 = 'Q',
        Feedbacker = 'F',
        Knuckleblaster = 'K',
        ChangeFist = 'G',
        Whiplash = 'R',
    }
}
