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

namespace sea_battle_C_
{
    public partial class Form1 : Form
    {
        BattleShip player1;
        BattleShip player2;
        Timer timer = new Timer();
        public Form1()
        {
            AllocConsole();
            InitializeComponent();
            player1 = new BattleShip(this, FacingDirection.Right);
            player2 = new BattleShip(this, FacingDirection.Left);
            this.ClientSize = new Size(Constants.FormWidth, Constants.FormHeight);
            this.ResizeRedraw = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            InitializeShip(player1, new Point(0, Constants.FormHeight / 2));
            InitializeShip(player2, new Point(Constants.FormWidth - player1.Width, Constants.FormHeight / 2));
            InitializeTimer();
            timer.Start();
            Console.WriteLine($"Window Size: Width = {this.ClientSize.Width}, Height = {this.ClientSize.Height}");
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private void InitializeTimer()
        {
            timer.Interval = 1000/60;
            timer.Tick += new EventHandler(StepTimer);
        }

        public bool HasObstacle(Rectangle rectangle)
        {
            var region1 = player1.Bounds;
            var region2 = player2.Bounds;
            return region1.IntersectsWith(rectangle) || region2.IntersectsWith(rectangle);
        }

        private void InitializeShip(BattleShip ship, Point location)
        {
            ship.MaxX = this.ClientSize.Width;
            ship.MaxY = this.ClientSize.Height;
            ship.Location = location;
            Controls.Add(ship);
        }

        private void Form1_Load(object sender, EventArgs e) {}

        private void StepTimer(object sender, EventArgs e)
        {
            player1.MoveShip();
            player2.MoveShip();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("key: " + e.KeyCode);
            switch (e.KeyCode)
            {
                case Constants.Player1Controls.MoveRight:
                    player1.RightClicked();
                    break;
                case Constants.Player1Controls.MoveDown:
                    player1.DownClicked();
                    break;
                case Constants.Player1Controls.MoveLeft:
                    player1.LeftClicked();
                    break;
                case Constants.Player1Controls.MoveUp:
                    player1.UpClicked();
                    break;
                case Constants.Player2Controls.MoveRight:
                    player2.RightClicked();
                    break;
                case Constants.Player2Controls.MoveDown:
                    player2.DownClicked();
                    break;
                case Constants.Player2Controls.MoveLeft:
                    player2.LeftClicked();
                    break;
                case Constants.Player2Controls.MoveUp:
                    player2.UpClicked();
                    break;
            }
        }
    }
}
