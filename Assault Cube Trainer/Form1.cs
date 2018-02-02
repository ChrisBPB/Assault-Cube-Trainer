using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Assault_Cube_Trainer
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            Process ac = Process.GetProcessesByName("ac_client")[0];
            int gameBase = ac.MainModule.BaseAddress.ToInt32();
            //int localPlayerOffset = 0x10f4f4;

            Memory pm = new Memory();
            pm.ReadProcess = ac;
            pm.OpenProcess();

            //PlayerEntity localPlayer = new PlayerEntity(0x0B9916C0, pm);
            //localPlayer.loadPlayerData();
            GameManager gm = new GameManager(gameBase, pm);
            //gm.loadNonLocalPlayers();

            Overlay o = new Overlay(pm, gm, "AssaultCube");
            o.Show();
            gm.startPlayerThread();
            o.setStarted(true);
        }
    }
}
