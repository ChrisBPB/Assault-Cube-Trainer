﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Assault_Cube_Trainer
{


    /**
     * This class is just throwing shit together. It is purely for testing. No judgements thanks (:
     * */
    public partial class frmMain : Form
    {

        #region cpp import

        public int VK_MOUSERIGHT = 0x02;
        GameManager gm;
        Overlay o;
        Memory pm;
        Process ac;
        int gameBase;

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            ac = Process.GetProcessesByName("ac_client")[0];
            gameBase = ac.MainModule.BaseAddress.ToInt32();

            pm = new Memory();
            pm.ReadProcess = ac;
            pm.OpenProcess();

            gm = new GameManager(gameBase, pm);

            o = new Overlay(pm, gm, "AssaultCube");
            o.Show();
            gm.startPlayerThread();
            o.setStarted(true);

        }

        private void frmMain_MouseDown(object sender, MouseEventArgs e)
        {
            //cant use this because form requires focus
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (GetKeyState(VK_MOUSERIGHT) == -128 || GetKeyState(VK_MOUSERIGHT) == -127)
            {
                //right mouse pressed
                gm.lockLocalToClosest();
                
            }
        }
    }
}
