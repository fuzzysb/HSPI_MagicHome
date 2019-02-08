using System.Drawing;

namespace MagicHomeAPI
{
    public static class DeviceExtensions
    {
        public static void SetColor(this Device device, Color color, byte? white1 = null, byte? white2 = null, bool waitForResponse = true, bool persist = true,  int timeOut = 100)
        {
            device.SetColor(color.R, color.G, color.B, white1, white2, waitForResponse, persist, timeOut);
        }
    }
}