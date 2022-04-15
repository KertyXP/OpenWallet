using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace OpenWallet.Common
{
    public static class ColorExtension
    {

        public static Color SetAlpha(this Color color, int alpha)
        {
            return Color.FromArgb(alpha, color);
        }
    }
}
