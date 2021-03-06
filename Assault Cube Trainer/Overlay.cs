﻿using System;
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
 
    /**
     * POINTS of our rect x,y x,y
     */ 
    public struct POINTS
    {
        public int Left, Top, Right, Bottom;
    }

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

        
        

        public string WindowName; //name of window to get the handle to
        POINTS gameWindow;        //x,y x,y of our Game Window

        bool started = false;     //has drawing started?

        public Memory pm;         //our memory object
        public GameManager gm;    //the game manager object

        public Overlay(Memory pm, GameManager gm, string windowName)
        {
            this.pm = pm;
            this.gm = gm;
            this.WindowName = windowName;
            InitializeComponent();
            this.TransparencyKey = this.BackColor = Color.Chocolate; //makes it transparent
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
                if (gm.espEntities != null) //if we have stuff to draw
                {
                    Point from = new Point((gameWindow.Right - (gameWindow.Left / 2)), gameWindow.Bottom); //bottom center of screen
                    foreach (KeyValuePair<LocatableEntity, LocatableEntity[]> entity in gm.espEntities)
                    {
                        for (int i = 0; i < entity.Value.Length; i++)
                        {
                            byte[] matrix = gm.viewMatrix;
                            if (matrix!=null && entity.Value[i] != null)
                            {
                                
                                float[] xyFoot = Calculations.WorldToScreen(matrix, entity.Value[i], gameWindow, true); //we get the head and foot because
                                float[] xyHead = Calculations.WorldToScreen(matrix, entity.Value[i], gameWindow, false);//we use it to calc height of player for esp box
                                if (xyFoot != null && xyHead != null && entity.Value[i].valid())
                                {                                      
                                        int colorWash = entity.Value[i] is Player ? ((Player)entity.Value[i]).health : 1; //we use this to change the red intensity depending on health if it's a player
                                        if (colorWash > 0)
                                        {
                                            pen.Color = Color.FromArgb(255, 255 - colorWash, colorWash, 0);

                                            //ESP line
                                            Point to = new Point(((int)(gameWindow.Left + xyFoot[0])), (int)(gameWindow.Top + xyFoot[1]));
                                            g.DrawLine(pen, from, to);

                                            //ESP Box
                                            float height = Math.Abs(xyHead[1] - xyFoot[1]); //get the height of our Entity
                                            float width = height / 2;            //players don't expand so we can just trial and error the width based on height
                                            g.DrawRectangle(pen, (gameWindow.Left + (xyHead[0] - width / 2)), (gameWindow.Top + (xyHead[1])), width, height);
                                        }
                                }
                            }
                           
                        }                      
                    }
                }
            }
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
