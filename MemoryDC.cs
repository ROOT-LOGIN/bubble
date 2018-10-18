/*
RELEASE TO PUBLIC DOMAIN!
NO ANY WARRANTY!
USE ON YOUR OWN RISK!
*/

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Drawing
{
    public class MemoryDC : IDisposable
    {
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        extern static IntPtr CreateCompatibleDC(IntPtr hdc);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        extern static IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hobj);

        IntPtr oldbmp;
        IntPtr HDC;

        public void Dispose()
        {
            SelectObject(Graphics.GetHdc(), oldbmp);
            Graphics.ReleaseHdc(); // Must release dc
        }

        public MemoryDC(Graphics g, int w, int h)
        {
            var dc = g.GetHdc();
            var hbmp = CreateCompatibleBitmap(dc, w, h);
            HDC = CreateCompatibleDC(dc);
            oldbmp = SelectObject(HDC, hbmp);

            this.Graphics = Graphics.FromHdc(HDC);
            g.ReleaseHdc(); // Release DC is very important
        }

        public Graphics Graphics
        {
            get; private set;
        }

        public static implicit operator IntPtr(MemoryDC dc)
        {
            return dc.HDC;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public extern static int BitBlt(
            IntPtr hdcDest,
            int nXDest,
            int nYDest,
            int nWidth,
            int nHeight,
            IntPtr hdcSrc,
            int nXSrc,
            int nYSrc,
            uint dwRop
        );

        /// <summary>
        /// Intend to paint content to a compatible dc and the save as an image
        /// </summary>        
        public static void OnPaint(object OO, PaintEventArgs EE)
        {
            var StOnPaint = statusBar.GetType().GetMethod("OnPaint", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var img = new Bitmap(StatusBar.Width, StatusBar.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var gi = Graphics.FromImage(img); // this is not a compatible dc, so, gdi antialias will not take effect.
            
            var mdc = new MemoryDC(EE.Graphics, StatusBar.Width, StatusBar.Height); // create a compatible dc
            var g = mdc.Graphics; // Graphics.FromHdc(EE.Graphics.GetHdc());
            g.Clear(Color.FromArgb(0, 1, 1, 1));
            g.Clear(r_defaultStatusBarBackColor);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.GammaCorrected;
            StOnPaint.Invoke(StatusBar, new[] { new PaintEventArgs(g, EE.ClipRectangle) }); // draw on the compatible dc, so gdi antialias will take effect

            MemoryDC.BitBlt(gi.GetHdc(), 0, 0, img.Width, img.Height, mdc, 0, 0, 0xCC0020); // copy the compatible dc content to the image dc
            gi.ReleaseHdc(); // realse dc is very important
            gi.Flush();

            img.Save("sta.bmp", System.Drawing.Imaging.ImageFormat.Bmp); // now we can save the image data
            img.Save("sta.png", System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
