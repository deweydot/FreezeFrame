namespace FFClient.AST
{
    public readonly ref struct Cursor(ReadOnlySpan<char> span, int position)
    {
        private readonly ReadOnlySpan<char> span = span;
        private readonly int position = position;
        public int Position => position;
        public Cursor TrimStart(ReadOnlySpan<char> trimElements)
        {
            int before = span.Length;
            ReadOnlySpan<char> trimmed = span.TrimStart(trimElements);
            return new Cursor(trimmed, position + before - trimmed.Length);
        }
        public Cursor TrimEnd(ReadOnlySpan<char> trimElements) => new Cursor(span.TrimEnd(trimElements), position);
        public Cursor Trim(ReadOnlySpan<char> trimElements) => TrimStart(trimElements).TrimEnd(trimElements);
        public Cursor Slice(int start) => new Cursor(span.Slice(start), position + start);
        public Cursor Slice(int start, int length) => new Cursor(span.Slice(start, length), position + start);
        public ref readonly char this[int index] => ref span[index];
        public int Length => span.Length;
        public override string ToString() => span.ToString();
        public static implicit operator ReadOnlySpan<char>(Cursor c) => c.span;
        public int IndexOf(char value) => span.IndexOf(value);
        public int IndexOfAny(ReadOnlySpan<char> values) => span.IndexOfAny(values);
        public bool SequenceEqual(ReadOnlySpan<char> other) => span.SequenceEqual(other);
        public bool StartsWith(char value) => span.StartsWith(value);
        public bool StartsWith(ReadOnlySpan<char> value) => span.StartsWith(value);
        public Cursor TrimAfter(char separator)
        {
            int index = IndexOf(separator);
            if (index == -1) return this;
            return this[..index];
        }
        public Cursor TrimAfter(ReadOnlySpan<char> separators)
        {
            int index = IndexOfAny(separators);
            if (index == -1) return this;
            return this[..index];
        }
    }
}
