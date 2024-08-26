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
                Func<bool?>? textIsBackground,
                UIColor background,
                UIColor backgroundDisabled,
                UIColor backgroundHover,
                UIColor backgroundActive,
                UIColor border,
                UIColor text,
                UIColor textSecondary,
                UIColor textDisabled
                )
            {
                TextIsBackground_delegate = textIsBackground;
                Background = background;
                BackgroundDisabled = backgroundDisabled;
                BackgroundHover = backgroundHover;
                BackgroundActive = backgroundActive;
                Border = border;
                Text = text;
                TextSecondary = textSecondary;
                TextDisabled = textDisabled;
            }
            
            internal ThemeButtonColor(
                UIColor background,
                UIColor backgroundDisabled,
                UIColor backgroundHover,
                UIColor backgroundActive,
                UIColor border,
                UIColor text,
                UIColor textSecondary,
                UIColor textDisabled
                ) : this(null,
                    background,
                    backgroundDisabled,
                    backgroundHover,
                    backgroundActive,
                    border,
                    text,
                    textSecondary,
                    textDisabled)
            {
            }

            private Func<bool?>? TextIsBackground_delegate;
            public bool TextIsBackground { get => TextIsBackground_delegate?.Invoke() ?? false; }
            public UIColor Background { get; }
            public UIColor BackgroundDisabled { get; }
            public UIColor BackgroundHover { get; }
            public UIColor BackgroundActive { get; }
            public UIColor Border { get; }
            public UIColor Text { get; }
            public UIColor TextSecondary { get; }
            public UIColor TextDisabled { get; }
        }

        internal class ThemeFile
        {
            internal class ButtonColorFile
            {
                [JsonPropertyName("text_is_background")]
                public bool TextIsBackground { get; set; }
                [JsonPropertyName("background")]
                public string? Background { get; set; }
                [JsonPropertyName("background_disabled")]
                public string? BackgroundDisabled { get; set; }
                [JsonPropertyName("background_hover")]
                public string? BackgroundHover { get; set; }
                [JsonPropertyName("background_active")]
                public string? BackgroundActive { get; set; }
                [JsonPropertyName("border")]
                public string? Border { get; set; }

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
                StatusOk = "#2F6D30",
                StatusError = "#6E3131",
                Neutral = new()
                {
                    TextIsBackground = false,
                    Background = "#383838",
                    BackgroundDisabled = "#242424",
                    BackgroundHover = "#424242",
                    BackgroundActive = "#4d4d4d",
                    Border = "#797979",
                    Text = "#fff",
                    TextSecondary = "#BCBCBC",
                    TextDisabled = "#b3b3b3"
                },
                Outline = new()
                {
                    TextIsBackground = false,
                    Background = "#0F0F0F",
                    BackgroundDisabled = "#0F0F0F",
                    BackgroundHover = "#1a1a1a",
                    BackgroundActive = "#262626",
                    Border = "#797979",
                    Text = "#BCBCBC",
                    TextSecondary = "#858585",
                    TextDisabled = "#b3b3b3"
                },
                Primary = new()
                {
                    TextIsBackground = false,
                    Background = "#2e65c9",
                    BackgroundDisabled = "#0e2247",
                    BackgroundHover = "#3b76e3",
                    BackgroundActive = "#2a5dbd",
                    Border = "#7EA6ED",
                    Text = "#fff",
                    TextSecondary = "#BCBCBC",
                    TextDisabled = "#b3b3b3"
                },
                Success = new()
                {
                    TextIsBackground = false,
                    Background = "#0d1a0e",
                    BackgroundDisabled = "#151c16",
                    BackgroundHover = "#152b17",
                    BackgroundActive = "#1b381d",
                    Border = "#797979",
                    Text = "#c3f7be",
                    TextSecondary = "#7eab79",
                    TextDisabled = "#72916e"
                },
                Error = new()
                {
                    TextIsBackground = false,
                    Background = "#1f0d0d",
                    BackgroundDisabled = "#170404",
                    BackgroundHover = "#331212",
                    BackgroundActive = "#421717",
                    Border = "#797979",
                    Text = "#f2a7a7",
                    TextSecondary = "#d97e7e",
                    TextDisabled = "#8c6565"
                },
                Warning = new()
                {
                    TextIsBackground = false,
                    Background = "#241f0a",
                    BackgroundDisabled = "#242115",
                    BackgroundHover = "#362f0f",
                    BackgroundActive = "#4a4010",
                    Border = "#797979",
                    Text = "#edcc47",
                    TextSecondary = "#ded4ad",
                    TextDisabled = "#a39b7c"
                },
                TextSecondaryButton = new()
                {
                    TextIsBackground = true,
                    Background = "#383838",
                    BackgroundDisabled = "#242424",
                    BackgroundHover = "#424242",
                    BackgroundActive = "#4d4d4d",
                    Border = "#797979",
                    Text = "#fff",
                    TextSecondary = "#BCBCBC",
                    TextDisabled = "#b3b3b3"
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
            [JsonPropertyName("status_ok")]
            public string? StatusOk { get; set; }
            [JsonPropertyName("status_error")]
            public string? StatusError { get; set; }
            [JsonPropertyName("neutral")]
            public ButtonColorFile? Neutral { get; set; }
            [JsonPropertyName("outline")]
            public ButtonColorFile? Outline { get; set; }
            [JsonPropertyName("primary")]
            public ButtonColorFile? Primary { get; set; }
            [JsonPropertyName("success")]
            public ButtonColorFile? Success { get; set; }
            [JsonPropertyName("error")]
            public ButtonColorFile? Error { get; set; }
            [JsonPropertyName("warning")]
            public ButtonColorFile? Warning { get; set; }
            [JsonPropertyName("text_secondary_button")]
            public ButtonColorFile? TextSecondaryButton { get; set; }
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
        private static DelegateUIColor _StatusOk = new DelegateUIColor(() => ParseColor(current?.StatusOk));
        public static UIColor StatusOk => _StatusOk;
        private static DelegateUIColor _StatusError = new DelegateUIColor(() => ParseColor(current?.StatusError));
        public static UIColor StatusError => _StatusError;
        private static ThemeButtonColor _Neutral = new(
            () => current?.Neutral?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Neutral?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Neutral?.TextDisabled))
            );
        public static ThemeButtonColor Neutral => _Neutral;
        private static ThemeButtonColor _Outline = new(
            () => current?.Outline?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Outline?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Outline?.TextDisabled))
            );
        public static ThemeButtonColor Outline => _Outline;
        private static ThemeButtonColor _Primary = new(
            () => current?.Primary?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Primary?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Primary?.TextDisabled))
            );
        public static ThemeButtonColor Primary => _Primary;
        private static ThemeButtonColor _Success = new(
            () => current?.Success?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Success?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Success?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Success?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Success?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Success?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Success?.TextDisabled))
            );
        public static ThemeButtonColor Success => _Success;
        private static ThemeButtonColor _Error = new(
            () => current?.Error?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Error?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Error?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Error?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Error?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Error?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Error?.TextDisabled))
            );
        public static ThemeButtonColor Error => _Error;
        private static ThemeButtonColor _Warning = new(
            () => current?.Warning?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.Warning?.Background)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.Border)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.Text)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.Warning?.TextDisabled))
            );
        public static ThemeButtonColor Warning => _Warning;
        private static ThemeButtonColor _TextSecondaryButton = new(
            () => current?.TextSecondaryButton?.TextIsBackground,
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.Background)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.BackgroundDisabled)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.BackgroundHover)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.BackgroundActive)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.Border)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.Text)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.TextSecondary)),
            new DelegateUIColor(() => ParseColor(current?.TextSecondaryButton?.TextDisabled))
            );
        public static ThemeButtonColor TextSecondaryButton => _TextSecondaryButton;

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
