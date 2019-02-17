using System.ComponentModel;

namespace MagicHomeAPI
{
    public enum DeviceType
    {
        [Description("Rgb")] Rgb = 0,
        [Description("RgbWarmwhite")] RgbWarmwhite = 1,
        [Description("RgbWarmwhiteCoolwhite")] RgbWarmwhiteCoolwhite = 2,
        [Description("Bulb")] Bulb = 3, // V.4+
        [Description("LegacyBulb")] LegacyBulb = 4, // V.3-
        [Description("LegacyRgbWarmwhiteCoolwhite")]
        LegacyRgbWarmwhiteCoolwhite = 5,
        [Description("LegacyRgb")]
        LegacyRgb = 6,
        [Description("WarmwhiteCoolwhite")]
        WarmwhiteCoolwhite = 7

    }
}
