namespace FFClient.AST
{
    class Parser
    {
        private ScintillaNET.Scintilla scintilla;
        private string fullText;
        private const string whitespace = " \t";

        public Parser(ScintillaNET.Scintilla scintilla)
        {
            this.scintilla = scintilla;
            fullText = scintilla.Text;
        }

        public List<Node> Parse()
        {
            var nodes = new List<Node>();
            var exceptions = new List<ParserException>();
            foreach (var line in scintilla.Lines)
            {
                try { nodes.Add(ParseLine(line)); }
                catch (ParserException e) { exceptions.Add(e); }
            }
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
            return nodes;
        }

        private Node ParseLine(ScintillaNET.Line line)
        {
            Cursor cur = new Cursor(fullText.AsSpan(line.Position, line.Length), line.Position); // get span
            cur = cur.TrimAfter(';'); // trim comment
            cur = cur.Trim(whitespace); // trim whitespace
            if (cur.StartsWith("##")) return ParseLoadout(cur); // match select loadout
            else if (cur.StartsWith("#")) return ParseLevelStart(cur); // match level start
            else if (cur.SequenceEqual("***")) return new Breakpoint() { Span = (cur.Position, 3) }; // match breakpoint
            return ParseInputLine(cur); // match input
        }

        private Node ParseLoadout(Cursor cur)
        {
            throw new NotImplementedException();
        }

        private LevelStart ParseLevelStart(Cursor cur)
        {
            Cursor name = cur[1..].TrimStart(whitespace);
            if (name.Length == 0) throw new ParserException("Level name not provided", (cur.Position, cur.Length));
            return new LevelStart(name.ToString()) { Span = (name.Position, name.Length) };
        }

        private Input ParseInputLine(Cursor cur)
        {
            IntValue count = ParseCount(cur);
            cur = cur[count.Span.Length..].TrimStart(whitespace);
            Aim aim = ParseAim(cur);
            cur = cur[aim.Span.Length..].TrimStart(whitespace);
            List<Button> buttons = ParseButtons(cur);
            return new Input(count, aim, buttons) { Span = (cur.Position, cur.Length) };
        }

        private IntValue ParseCount(Cursor cur)
        {
            cur = cur.TrimAfter(whitespace);
            if (!int.TryParse(cur, out int val)) throw new ParserException("Failed to parse frame count as an integer.", (cur.Position, cur.Length));
            return new IntValue(val) { Span = (cur.Position, cur.Length) };
        }

        private Aim ParseAim(Cursor cur)
        {
            AimVector vec1 = ParseAimVector(cur);
            cur = cur[vec1.Span.Length..].TrimStart(whitespace);
            if (!cur.StartsWith("->")) return new AimDiscrete(vec1) { Span = vec1.Span };
            cur = cur[2..].TrimStart(whitespace);
            AimVector vec2 = ParseAimVector(cur);
            return new AimContinuous(vec1, vec2) { Span = (cur.Position, vec2.Span.Position - cur.Position + vec2.Span.Length) };
        }

        private AimVector ParseAimVector(Cursor cur)
        {
            if (cur.StartsWith('_')) return new AimVectorBlank() { Span = (cur.Position, 1) }; // handle underscore
            bool abs = cur.StartsWith('@'); // handle leading at sign
            Cursor coord = abs ? cur[1..] : cur;
            Cursor x = coord.TrimAfter(whitespace + ','); // parse x value
            if (!float.TryParse(x, out float xVal)) throw new ParserException("Failed to parse coordinate value as a float.", (x.Position, x.Length));
            coord = coord[x.Length..].TrimStart(whitespace);
            if (coord.StartsWith(',')) coord = coord[1..].TrimStart(whitespace); // handle seperator
            else throw new ParserException("Expected a ',' in aim coordinate.", (coord.Position, coord.IndexOfAny(whitespace)));
            Cursor y = abs ? coord.TrimAfter(whitespace + ',') : coord.TrimAfter(whitespace); // parse y value
            if (!float.TryParse(y, out float yVal)) throw new ParserException("Failed to parse coordinate value as a float.", (y.Position, y.Length));
            coord = coord[y.Length..].TrimStart(whitespace);
            if (abs) // handle seperator
            {
                if (!coord.StartsWith(',')) throw new ParserException("Expected 3 values in aim coordinate but found only 2.", (cur.Position, y.Position - cur.Position + y.Length));
                coord = coord[1..].TrimStart(whitespace);
                Cursor z = coord.TrimAfter(whitespace); // parse z value
                if (!float.TryParse(z, out float zVal)) throw new ParserException("Failed to parse coordinate value as a float.", (z.Position, z.Length));
                return new AimVectorPoint(
                        new FloatValue(xVal) { Span = (x.Position, x.Length) },
                        new FloatValue(yVal) { Span = (y.Position, y.Length) },
                        new FloatValue(zVal) { Span = (z.Position, z.Length) }
                    ) { Span = (cur.Position, z.Position - cur.Position + z.Length) };
            }
            return new AimVectorAngle(
                    new FloatValue(xVal) { Span = (x.Position, x.Length) },
                    new FloatValue(yVal) { Span = (y.Position, y.Length) }
                ) { Span = (cur.Position, y.Position - cur.Position + y.Length) };
        }

        private List<Button> ParseButtons(Cursor cur)
        {
            Cursor keyCode; Key key;
            List<Button> buttons = new List<Button>();
            while (cur.Length > 1)
            {
                keyCode = cur.TrimAfter(whitespace);
                key = (Key) char.ToUpper(keyCode[0]);
                if (!Enum.IsDefined(key)) throw new ParserException("Character code provided is not a defined key.", (keyCode.Position, keyCode.Length));
                if (keyCode.Length > 1)
                {
                    if (keyCode[1..].SequenceEqual(".")) buttons.Add(new ButtonHoldImplicit(key) { Span = (keyCode.Position, 2) });
                    else if (!int.TryParse(keyCode[1..], out int val)) throw new ParserException("Hold duration could not be parsed into an integer.", (keyCode.Position, keyCode.Length));
                    else buttons.Add(new ButtonHoldExplicit(key, val) { Span = (keyCode.Position, keyCode.Length)});
                }
                else
                {
                    if (char.IsUpper(keyCode[0])) buttons.Add(new ButtonPress(key) { Span = (keyCode.Position, 1)});
                    else buttons.Add(new ButtonRelease(key) { Span = (keyCode.Position, 1) });
                }
                cur = cur[keyCode.Length..].TrimStart(whitespace);
            }
            return buttons;
        }

        class ParserException : Exception
        {
            public (int, int) Span { get; }
            public ParserException(string message) : base(message) { }
            public ParserException(string message, (int, int) span) : base(message) { Span = span; }
        }
    }
}
