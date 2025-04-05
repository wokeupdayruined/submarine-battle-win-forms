using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sea_battle_C_
{
    static class Utils
    {
        public static void RemoveWhitePixels(ref Bitmap bitmap) {
            for (int x = 0; x < bitmap.Width; x++) {
                for (int y = 0; y < bitmap.Height; y++) {
                    Color color = bitmap.GetPixel(x, y);
                    if (color.R > 240 && color.G > 240 && color.B > 240) {
                        Console.WriteLine("Clearing white pixel at (" + x + ", " + y + ")");
                        bitmap.SetPixel(x, y, Color.Transparent);
                    }
                }
            }
        }
    }
}
