using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace piconavx.ui.graphics.ui
{
    public static class RichTextSegmentation
    {
        public struct TextSegmentWrapper(bool isDefault, TextSegment segment)
        {
            public bool IsDefault = isDefault;
            public TextSegment TextSegment = segment;
        }

        public const char ESCAPE_CHAR = '\x1b';

        public static readonly string Default = $"{ESCAPE_CHAR}[0m";
        public static readonly string TextSecondary = $"{ESCAPE_CHAR}[1;TextSecondarym";

        public static bool TryParseSequence(ref string text, ref int start, int visibleIndex, [NotNullWhen(true)] out TextSegmentWrapper? segment)
        {
            if (text[start] == ESCAPE_CHAR)
            {
                if (text[start+1] == '[')
                {
                    int end = text.IndexOf('m', start + 2);
                    if (end != -1)
                    {
                        string[] parts = text[(start + 2)..end].Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                        start = end;
                        if (parts.Length > 0 && int.TryParse(parts[0], out int type))
                        {
                            switch (type)
                            {
                                case 0:
                                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                        segment = new TextSegmentWrapper(true, new(new(visibleIndex, visibleIndex), null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                        return true;
                                    }
                                case 1:
                                    {
                                        if (parts.Length > 1)
                                        {
                                            if (parts[1].Equals("TextSecondary", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                segment = new TextSegmentWrapper(false, new(new(visibleIndex, visibleIndex), Theme.TextSecondary));
                                                return true;
                                            }
                                        }
                                        break;
                                    }
                            }
                        }
                    }
                }
            }

            segment = null;
            return false;
        }

        public static (string visibleText, TextSegment[]? segments) Segment(string text)
        {
            List<TextSegmentWrapper> startSegments = [];
            StringBuilder visible = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (TryParseSequence(ref text, ref i, visible.Length, out var segment))
                {
                    startSegments.Add(segment.Value);
                }
                else
                {
                    visible.Append(text[i]);
                }
            }

            for (int i = startSegments.Count - 1; i >= 0; i--)
            {
                if (!startSegments[i].IsDefault)
                {
                    startSegments[i] = new(false, new(new Range(startSegments[i].TextSegment.Range.Start, (i < startSegments.Count - 1) ? (startSegments[i + 1].TextSegment.Range.Start.Value) : visible.Length), startSegments[i].TextSegment.Color));
                }
            }

            return (visible.ToString(), startSegments.Where(v => !v.IsDefault).Select(v => v.TextSegment).ToArray());
        }
    }
}
