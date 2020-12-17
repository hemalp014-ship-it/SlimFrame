using SlimDX;
using SlimDX.Direct2D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlimFrame.UI
{
    public class DXContolImages : IDisposable
    {
        SlimDX.Direct2D.Bitmap m_UI; // Holds the tile sprite sheet that we use to draw the world.

        RectangleF rectSqr;

        RectangleF rectBar;

        float Height;
        float Width;
        float BarWidth;

        public SlimDX.Direct2D.Bitmap Image
        {
            get
            {
                return m_UI;
            }
        }

        public RectangleF RadioButtonBase
        {
            get
            {
                rectSqr.X = 0;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF RadioButtonCheck
        {
            get
            {
                rectSqr.X = Width;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF CheckBoxBase
        {
            get
            {
                rectSqr.X = Width * 2;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF CheckBoxCheck
        {
            get
            {
                rectSqr.X = Width * 3;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF ComboBoxDown
        {
            get
            {
                rectSqr.X = Width * 4;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF ComboBoxUp
        {
            get
            {
                rectSqr.X = Width * 5;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;
                return rectSqr;
            }
        }

        public RectangleF SliderPointer
        {
            get
            {
                rectSqr.X = Width * 6;
                rectSqr.Y = 0;

                rectSqr.Width = Width;
                rectSqr.Height = Height;

                return rectSqr;
            }
        }

        public RectangleF SliderBase
        {
            get
            {
                rectBar.X = 0;
                rectBar.Y = (Height + 1) + 5 ;

                rectBar.Width = BarWidth;
                rectBar.Height = (75 - rectBar.Y) * 2;

                return rectBar;
            }
        }

        public RectangleF ScrollerBase
        {
            get
            {
                rectBar.X = 0;
                rectBar.Y = 2 * (Height + 1) + 11;

                rectBar.Width = BarWidth - 1;
                rectBar.Height = 28;

                return rectBar;
            }
        }

        public RectangleF ScrollerPointer
        {
            get
            {
                rectBar.X = 200;
                rectBar.Y = 2 * (Height + 1) + 11;

                rectBar.Width = BarWidth;
                rectBar.Height = 28;

                return rectBar;
            }
        }

        public RectangleF ButtonBase
        {
            get
            {
                rectBar.X = rectBar.Width;
                rectBar.Y = (Height + 1);

                rectBar.Width = BarWidth;
                rectBar.Height = Height;

                return rectBar;
            }
        }

        public DXContolImages(RenderTarget rt)
        {
            rectSqr = new RectangleF();
            rectBar = new RectangleF();

            Width = 50;
            Height = 50;
            BarWidth = 200;
            
            rectSqr.Width = 50;
            rectSqr.Height = 50;

            rectBar.Width = 200;
            rectBar.Height = 50;

            m_UI = System2D.LoadBitmap(rt, "Assets\\UI\\UI.png");
        }

        public void DrawImage(RenderTarget rt, RectangleF source, RectangleF dest, float alpha = 1.0f, InterpolationMode mode = InterpolationMode.Linear)
        {
            rt.DrawBitmap(m_UI, dest, alpha, mode, source);
        }

        public void DrawImageTranspose(RenderTarget rt, RectangleF source, RectangleF dest, float alpha = 1.0f, InterpolationMode mode = InterpolationMode.Linear)
        {
            float x, y, w, h;

            x = dest.X;
            y = dest.Y;
            w = dest.Width;
            h = dest.Height;

            PointF center = new PointF(dest.X + w / 2, dest.Y + h / 2);
            //source.X += source.Width;
            //source.Y += source.Height;
            //source.Width = -source.Width;
            //source.Height = -source.Height;

            rt.Transform = Matrix3x2.Rotation(-90, center) * Matrix3x2.Scale(1, -1, center);

            dest.Width = h;
            dest.Height = w;
            dest.Y -= -(h / 2 - w / 2);
            dest.X -= h / 2 - w / 2;

            rt.DrawBitmap(m_UI, dest, alpha, mode, source);
            rt.Transform = Matrix3x2.Identity;

            dest.Width = w;
            dest.Height = h;

            dest.X = x;
            dest.Y = y;
        }

        #region IDisposable Support
        private bool m_Disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                    m_UI?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                m_Disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ContolImages()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}