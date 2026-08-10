using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
namespace FFClient
{
    class ParserException : Exception
    {
        public ParserException(string message) : base(message) { }
        public required int Line { get; init; }
        public required int Offset { get; init; }
        public required int Length { get; init; }
    }

    static partial class Parser
    {
        public static List<AST.Node> Parse(IEnumerable<ScintillaNET.Line> lines)
        {
            List<AST.Node> nodes = new List<AST.Node>();
            foreach (var line in lines)
            {
                nodes.Add(ParseLine(line));
            }
            return nodes;
        }

        [GeneratedRegex(@"^\s*(?<body>[^;]*?)\s*(?<comment>;[^\r\n]*)?$", RegexOptions.ExplicitCapture)]
        private static partial Regex StripLine();

        [GeneratedRegex(@"\G#\s*(?<level>\S+)", RegexOptions.ExplicitCapture)]
        private static partial Regex LevelStart();

        [GeneratedRegex(@"\G(?<num>\d+)", RegexOptions.ExplicitCapture)]
        private static partial Regex FrameCount();

        [GeneratedRegex(@"\G\s*((?<blank>_)|(?<at>@)?(?<x>\S+?)\s*,\s*(?<y>\S+?)(\s*,\s*(?<z>\S+))?)", RegexOptions.ExplicitCapture)]
        private static partial Regex AimCoord();

        [GeneratedRegex(@"\G\s*->")]
        private static partial Regex ArrowPattern();

        private static AST.Node ParseLine(ScintillaNET.Line line)
        {
            string text = line.Text;
            int lineNumber = line.Index;

            // strip indent and comment
            Group stripped = StripLine().Match(text).Groups["body"];
            if (!stripped.Success) throw new InvalidDataException("how tf?");
            ReadOnlySpan<char> body = stripped.ValueSpan;
            int bodyStart = stripped.Index;
            int bodyLength = stripped.Length;

            // match select loadout
            if (body.StartsWith("##")) return ParseLoadout(stripped.Value, line.Index, stripped.Index);

            // match level start
            else if (body.StartsWith("#")) return ParseLevelStart(stripped.Value, line.Index, stripped.Index);

            // match breakpoint
            else if (body.SequenceEqual("***")) return new AST.Breakpoint() { Row = lineNumber, Col = bodyStart, Length = 3 };

            // match input
            else
            {
                // get frame count
                Group frame = FrameCount().Match(text, bodyStart, bodyLength).Groups["num"];
                if (!frame.Success) throw new InvalidDataException("unrecognized syntax");

                var frameCount = new AST.FrameCount(int.Parse(frame.ValueSpan)) { Row = lineNumber, Col = frame.Index, Length = frame.Length };
                int offset = frame.Index + frame.Length;

                // get mouse position
                GroupCollection coord = AimCoord().Match(text, offset, bodyStart + bodyLength - offset).Groups;
                AST.Aim aim;
                if (coord["blank"].Success) {
                    aim = new AST.AimBlank() { Row = lineNumber, Col = coord["blank"].Index, Length = 1 };
                }
                else if (coord["coord"].Success)
                {
                    AST.AimVector vec1;
                    if (coord["at"].Success)
                    {
                        if (coord["x"].Success && coord["y"].Success && coord["z"].Success)
                        {
                            if (!float.TryParse(coord["x"].ValueSpan, out float x)) throw new InvalidDataException("unrecognized syntax");
                            else
                                if (!float.TryParse(coord["y"].ValueSpan, out float y)) throw new InvalidDataException("unrecognized syntax");
                            if (!float.TryParse(coord["z"].ValueSpan, out float z)) throw new InvalidDataException("unrecognized syntax");
                            vec1 = AST.AimVectorPoint(x, y, z) { };
                        }
                        else throw new InvalidDataException("unrecognized syntax");
                    }
                    else
                    {
                        if (coord["x"].Success && coord["y"].Success && !coord["z"].Success)
                        {
                            if (!float.TryParse(coord["x"].ValueSpan, out float x)) throw new InvalidDataException("unrecognized syntax");
                            if (!float.TryParse(coord["y"].ValueSpan, out float y)) throw new InvalidDataException("unrecognized syntax");
                        }
                        else throw new InvalidDataException("unrecognized syntax");
                    }
                }
                else throw new InvalidDataException("unrecognized syntax");



                return new AST.Input(
                        frameCount,
                        aim,
                        null
                    ) { Row = lineNumber, Col = bodyStart, Length = bodyLength };
            }
        }

        private static AST.Node ParseLoadout(string body, int line, int offset)
        {
            throw new NotImplementedException();
        }

        private static AST.LevelStart ParseLevelStart(string body, int line, int offset)
        {
            Group start = LevelStart().Match(body).Groups["level"];
            if (start.Success)
            {
                var levelName = new AST.LevelName(start.Value) { Row = line, Col = start.Index + offset, Length = start.Length };
                return new AST.LevelStart(levelName) { Row = line, Col = offset, Length = body.Length };
            }
            throw new ParserException("Level name is required") { Line = line, Offset = offset, Length = body.Length };
        }

        private static AST.Input ParseInputLine(string body, int line, int offset)
        {
            var 
            throw new NotImplementedException();
        }

        private static AST.FrameCount ParseFrameCount(string body, int line, int offset, out int end)
        {
            Group frame = FrameCount().Match(text, bodyStart, bodyLength).Groups["num"];
        }
    }
}
