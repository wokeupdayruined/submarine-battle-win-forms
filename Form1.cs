using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sea_battle_C_
{
    public partial class Form1 : Form
    {
        BattleShip ship = new BattleShip();
        Timer timer = new Timer();
        public Form1()
        {
            InitializeComponent();
            Controls.Add(ship);
            ship.MaxX = Width;
            ship.MaxY = Height;
            ship.Size = new System.Drawing.Size(40, 40);
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            timer.Interval = 1000/60;
            timer.Tick += new EventHandler(StepTimer);
            timer.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void StepTimer(object sender, EventArgs e)
        {
            ship.MoveShip();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.D)
            {
                ship.Direction = Direction.Right;
            } else if (e.KeyCode == Keys.S)
            {
                ship.Direction = Direction.Down;
            } else if (e.KeyCode == Keys.A)
            {
                ship.Direction = Direction.Left;
            }
            else if (e.KeyCode == Keys.W)
            {
                ship.Direction = Direction.Up;
            }
        }
    }
}
