using SixLabors.ImageSharp.PixelFormats;
using System.Text.Json.Serialization;
using static piconavx.ui.graphics.ui.Button;

namespace piconavx.ui.graphics.ui
{
    public static class Theme
    {
        private static readonly Rgba32 INVALID_COLOR = new(255, 0, 255, 255);

        public class ThemeButtonColor : ButtonColor
        {
            internal ThemeButtonColor(
                UIColor background,
                UIColor backgroundDisabled,
                UIColor backgroundHover,
                UIColor backgroundActive,
                UIColor text,
                UIColor textSecondary,
                UIColor textDisabled
                )
            {
                Background = background;
                BackgroundDisabled = backgroundDisabled;
                BackgroundHover = backgroundHover;
                BackgroundActive = backgroundActive;
                Text = text;
                TextSecondary = textSecondary;
                TextDisabled = textDisabled;
            }

            public UIColor Background { get; }
            public UIColor BackgroundDisabled { get; }
            public UIColor BackgroundHover { get; }
            public UIColor BackgroundActive { get; }
            public UIColor Text { get; }
            public UIColor TextSecondary { get; }
            public UIColor TextDisabled { get; }
        }

        internal class ThemeFile
        {
            internal class ButtonColorFile
            {
                [JsonPropertyName("background")]
                public string? Background { get; set; }
                [JsonPropertyName("background_disabled")]
                public string? BackgroundDisabled { get; set; }
                [JsonPropertyName("background_hover")]
                public string? BackgroundHover { get; set; }
                [JsonPropertyName("background_active")]
                public string? BackgroundActive { get; set; }

                [JsonPropertyName("text")]
                public string? Text { get; set; }
                [JsonPropertyName("text_secondary")]
                public string? TextSecondary { get; set; }
                [JsonPropertyName("text_disabled")]
                public string? TextDisabled { get; set; }
            }

            public static ThemeFile Default => new ThemeFile()
            {
                Background = "#0F0F0F",
                Viewport = "#212121",
                SidepanelBackground = "#0a0a0ac8",
                Header = "#FFF",
                Text = "#BCBCBC",
                TextSecondary = "#6C6C6C",
                TextSelection = "#1857BA",
                CardBackground = "#262626",
                CardShadow = "#00000080",
                CardThumbnail = "#FFF",
                CardThumbnailShadow = "#000000E6",
                TooltipShadow = "#00000080",
                ScrollThumb = "#383838",
                ScrollThumbHover = "#424242",
                ScrollThumbActive = "#4d4d4d",
                Neutral = new()
                {
                    Background = "#383838",
                    BackgroundDisabled = "#242424",
                    BackgroundHover = "#424242",
                    BackgroundActive = "#4d4d4d",
                    Text= "#fff",
                    TextSecondary = "#BCBCBC",
                    TextDisabled = "#b3b3b3"
                },
                Primary = new()
                {
                    Background = "#2e65c9",
                    BackgroundDisabled = "#0e2247",
                    BackgroundHover = "#3b76e3",
                    BackgroundActive = "#2a5dbd",
                    Text = "#fff",
                    TextSecondary = "#BCBCBC",
                    TextDisabled = "#b3b3b3"
                },
                Success = new()
                {
                    Background = "#0d1a0e",
                    BackgroundDisabled = "#151c16",
                    BackgroundHover = "#152b17",
                    BackgroundActive = "#1b381d",
                    Text = "#c3f7be",
                    TextSecondary = "#7eab79",
                    TextDisabled = "#72916e"
                }
            };

            [JsonPropertyName("background")]
            public string? Background { get; set; }
            [JsonPropertyName("viewport")]
            public string? Viewport { get; set; }
            [JsonPropertyName("sidepanel_background")]
            public string? SidepanelBackground { get; set; }
            [JsonPropertyName("header")]
            public string? Header { get; set; }
            [JsonPropertyName("text")]
            public string? Text { get; set; }
            [JsonPropertyName("text_secondary")]
            public string? TextSecondary { get; set; }
            [JsonPropertyName("text_selection")]
            public string? TextSelection { get; set; }
            [JsonPropertyName("card_background")]
            public string? CardBackground { get; set; }
            [JsonPropertyName("card_shadow")]
            public string? CardShadow { get; set; }
            [JsonPropertyName("card_thumbnail")]
            public string? CardThumbnail { get; set; }
            [JsonPropertyName("card_thumbnail_shadow")]
            public string? CardThumbnailShadow { get; set; }
            [JsonPropertyName("tooltip_shadow")]
            public string? TooltipShadow { get; set; }
            [JsonPropertyName("scroll_thumb")]
            public string? ScrollThumb { get; set; }
            [JsonPropertyName("scroll_thumb_hover")]
            public string? ScrollThumbHover { get; set; }
            [JsonPropertyName("scroll_thumb_active")]
            public string? ScrollThumbActive { get; set; }
            [JsonPropertyName("neutral")]
            public ButtonColorFile? Neutral { get; set; }
            [JsonPropertyName("primary")]
            public ButtonColorFile? Primary { get; set; }
            [JsonPropertyName("success")]
            public ButtonColorFile? Success { get; set; }
            [JsonPropertyName("error")]
            public ButtonColorFile? Error { get; set; }
        }

        private static DelegateUIColor _Background = new DelegateUIColor(() => ParseColor(current?.Background));
        public static UIColor Background => _Background;
        private static DelegateUIColor _Viewport = new DelegateUIColor(() => ParseColor(current?.Viewport));
        public static UIColor Viewport => _Viewport;
        private static DelegateUIColor _SidepanelBackground = new DelegateUIColor(() => ParseColor(current?.SidepanelBackground));
        public static UIColor SidepanelBackground => _SidepanelBackground;
        private static DelegateUIColor _Header = new DelegateUIColor(() => ParseColor(current?.Header));
        public static UIColor Header => _Header;
        private static DelegateUIColor _Text = new DelegateUIColor(() => ParseColor(current?.Text));
        public static UIColor Text => _Text;
        private static DelegateUIColor _TextSecondary = new DelegateUIColor(() => ParseColor(current?.TextSecondary));
        public static UIColor TextSecondary => _TextSecondary;
        private static DelegateUIColor _TextSelection = new DelegateUIColor(() => ParseColor(current?.TextSelection));
        public static UIColor TextSelection => _TextSelection;
        private static DelegateUIColor _CardBackground = new DelegateUIColor(() => ParseColor(current?.CardBackground));
        public static UIColor CardBackground => _CardBackground;
        private static DelegateUIColor _CardShadow = new DelegateUIColor(() => ParseColor(current?.CardShadow));
        public static UIColor CardShadow => _CardShadow;
        private static DelegateUIColor _CardThumbnail = new DelegateUIColor(() => ParseColor(current?.CardThumbnail));
        public static UIColor CardThumbnail => _CardThumbnail;
        private static DelegateUIColor _CardThumbnailShadow = new DelegateUIColor(() => ParseColor(current?.CardThumbnailShadow));
        public static UIColor CardThumbnailShadow => _CardThumbnailShadow;
        private static DelegateUIColor _TooltipShadow = new DelegateUIColor(() => ParseColor(current?.TooltipShadow));
        public static UIColor TooltipShadow => _TooltipShadow;
        private static DelegateUIColor _ScrollThumb = new DelegateUIColor(() => ParseColor(current?.ScrollThumb));
        public static UIColor ScrollThumb => _ScrollThumb;
        private static DelegateUIColor _ScrollThumbHover = new DelegateUIColor(() => ParseColor(current?.ScrollThumbHover));
        public static UIColor ScrollThumbHover => _ScrollThumbHover;
        private static DelegateUIColor _ScrollThumbActive = new DelegateUIColor(() => ParseColor(current?.ScrollThumbActive));
        public static UIColor ScrollThumbActive => _ScrollThumbActive;
        private static ThemeButtonColor _Neutral = new(
            new DelegateUIColor(() => ParseColor(current?.Neutral?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.TextDisabled))
            );
        public static ThemeButtonColor Neutral => _Neutral;
        private static ThemeButtonColor _Primary = new(
            new DelegateUIColor(() => ParseColor(current?.Primary?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.TextDisabled))
            );
        public static ThemeButtonColor Primary => _Primary;
        private static ThemeButtonColor _Success = new(
            new DelegateUIColor(() => ParseColor(current?.Success?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Success?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Success?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Success?.TextDisabled))
            );
        public static ThemeButtonColor Success => _Success;
        private static ThemeButtonColor _Error = new(
            new DelegateUIColor(() => ParseColor(current?.Error?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Error?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Error?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Error?.TextDisabled))
            );
        public static ThemeButtonColor Error => _Error;

        private static ThemeFile current;

        private static Rgba32 ParseColor(string? color)
        {
            if (Rgba32.TryParseHex(color, out var res))
            {
                return res;
            }
            else
            {
                return INVALID_COLOR;
            }
        }

        static Theme()
        {
            current = SavedResource.ReadTheme(SavedResource.Settings.Current.Theme);
        }

        public static void UpdateTheme()
        {
            current = SavedResource.ReadTheme(SavedResource.Settings.Current.Theme);
        }
    }
}
