using System.Drawing;

namespace BlazorWith3d.ExampleApp.Client.HTML;

public static class ColorUtils
{
    public static string ToCss(this Color c) => $"rgb({c.R}, {c.G}, {c.B});";

    public static Color Lerp(Color zeroColor, Color oneColor, float mix)
    {
        var iMix = 1 - mix;
        var r=zeroColor.R*iMix+oneColor.R*mix;
        var g=zeroColor.G*iMix+oneColor.G*mix;
        var b=zeroColor.B*iMix+oneColor.B*mix;
        var a=zeroColor.A*iMix+oneColor.A*mix;
        return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
    }
}