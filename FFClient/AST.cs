namespace FFClient.AST
{
    record struct Span(int Start, int Length);
    abstract record Node(Span Span);
    record LevelStart(Span Span, string level) : Node(Span);
    record BreakLine(Span Span) : Node(Span);
    record InputLine(Span Span, int Count, Aim Aim, ButtonsInput Buttons) : Node(Span);
    abstract record Aim(Span Span);
    record AimDiscrete(Span Span, AimVector Vec) : Aim(Span);
    record AimContinous(Span Span, AimVector Vec1, AimVector Vec2) : Aim(Span);
    record AimBlank(Span Span) : Aim(Span);
    abstract record AimVector(Span Span);
    record AimVectorAngle(Span Span, float X, float Y) : AimVector(Span);
    record AimVectorPoint(Span Span, float X, float Y, float Z) : AimVector(Span);
    record ButtonsInput(Span Span, List<Buttons> Buttons);
    abstract record Buttons(Span Span, Key Key);
    record ButtonPress(Span Span, Key Key) : Buttons(Span, Key);
    record ButtonRelease(Span Span, Key Key) : Buttons(Span, Key);
    record ButtonHold(Span Span, Key Key, int? Count) : Buttons(Span, Key);
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
