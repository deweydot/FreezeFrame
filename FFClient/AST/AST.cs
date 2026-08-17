namespace FFClient.AST
{
    abstract record Node
    {
        public required (int Position, int Length) Span { get; init; }
    };
    record LevelStart(string name) : Node();
    record Breakpoint() : Node();
    record Input(IntValue Count, Aim Aim, List<Button> Buttons) : Node();
    record IntValue(int Value) : Node();
    abstract record Aim() : Node();
    record AimDiscrete(AimVector Vec) : Aim();
    record AimContinuous(AimVector Vec1, AimVector Vec2) : Aim();
    abstract record AimVector() : Node();
    record AimVectorBlank() : AimVector();
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
