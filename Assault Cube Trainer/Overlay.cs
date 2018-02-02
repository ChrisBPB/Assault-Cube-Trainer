using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Assault_Cube_Trainer
{
    public partial class Overlay : Form
    {

        #region constants
        public const int WS_EX_LAYERED = 0x80000;
        public const int WS_EX_TRANSPARENT = 0x20;
        #endregion

        #region paint
        Font font = new Font(
                       new FontFamily("Arial"),
                       12,
                       FontStyle.Regular,
                       GraphicsUnit.Pixel);
        Brush brush = new SolidBrush(Color.Pink);
        Pen pen = new Pen(new SolidBrush(Color.Pink));
        #endregion

        #region cpp imports

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out POINTS lpPoints);
        #endregion

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTS
        {
            public int Left, Top, Right, Bottom;
        }

        public string WindowName;
        POINTS gameWindow;

        bool started = false;

        public Memory pm;
        public GameManager gm;

        public Overlay(Memory pm, GameManager gm, string windowName)
        {
            this.pm = pm;
            this.gm = gm;
            this.WindowName = windowName;
            InitializeComponent();
            this.TransparencyKey = this.BackColor = Color.Chocolate;
            WindowSetup();
        }

        public void setStarted(bool started)
        {
            this.started = started;
            timer1.Start();
        }

        private void Overlay_Paint(object sender, PaintEventArgs e)
        {
            if (started)
            {
                Graphics g = e.Graphics;
                if (gm.espLineEntities != null)
                {
                    Point from = new Point((gameWindow.Right - (gameWindow.Left / 2)), gameWindow.Bottom);
                    foreach (KeyValuePair<PlayerEntity, PlayerEntity[]> entity in gm.espLineEntities)
                    {
                        for (int i = 0; i < entity.Value.Length; i++)
                        {
                            if (entity.Value[i] != null)
                            {
                                float[] xy = WorldToScreen(entity.Value[i], gameWindow.Right - gameWindow.Left, gameWindow.Bottom - gameWindow.Top);
                                if (xy != null)
                                {
                                    Point to = new Point(((int)(gameWindow.Left + xy[0])), (int)(gameWindow.Top + xy[1]));
                                    g.DrawLine(pen, from, to);
                                }
                            }
                           
                        }                      
                    }
                }
            }
        }

        /**
         * 
         * World to screen function "borrowed" from 0XDE57
         * 
         * */
        public float[] WorldToScreen(PlayerEntity entity, int width, int height)
        {
            byte[] bfr = pm.ReadMatrix(gm.baseAddress + gm.offsets.viewMatrix);
            float m11 = BitConverter.ToSingle(bfr, 0), m12 = BitConverter.ToSingle(bfr, 4), m13 = BitConverter.ToSingle(bfr, 8), m14 = BitConverter.ToSingle(bfr, 12); //00, 01, 02, 03
            float m21 = BitConverter.ToSingle(bfr, 16), m22 = BitConverter.ToSingle(bfr, 20), m23 = BitConverter.ToSingle(bfr, 24), m24 = BitConverter.ToSingle(bfr, 28); //04, 05, 06, 07
            float m31 = BitConverter.ToSingle(bfr, 32), m32 = BitConverter.ToSingle(bfr, 36), m33 = BitConverter.ToSingle(bfr, 40), m34 = BitConverter.ToSingle(bfr, 44); //08, 09, 10, 11
            float m41 = BitConverter.ToSingle(bfr, 48), m42 = BitConverter.ToSingle(bfr, 52), m43 = BitConverter.ToSingle(bfr, 56), m44 = BitConverter.ToSingle(bfr, 60); //12, 13, 14, 15


            //multiply vector against matrix
            float screenX = (m11 * entity.xPos) + (m21 * entity.yPos) + (m31 * entity.zPosFoot) + m41;
            float screenY = (m12 * entity.xPos) + (m22 * entity.yPos) + (m32 * entity.zPosFoot) + m42;
            float screenW = (m14 * entity.xPos) + (m24 * entity.yPos) + (m34 * entity.zPosFoot) + m44;


            //camera position (eye level/middle of screen)
            float camX = width / 2f;
            float camY = height / 2f;

            //convert to homogeneous position
            float x = camX + (camX * screenX / screenW);
            float y = camY - (camY * screenY / screenW);
            float[] screenPos = { x, y };

            if (screenW > 0.001f)
            {
                return screenPos;
            }
            return null;
        }



        private void WindowSetup()
        {
            IntPtr handle = FindWindow(null, this.WindowName);
            if (handle != IntPtr.Zero)
            {
                //get the game window points
                GetWindowRect(handle, out gameWindow);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_LAYERED | WS_EX_TRANSPARENT;
                return cp;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Invalidate();
        }

    }
}
