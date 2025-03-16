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
    enum State
    {
        BeforeStart,
        InProgress,
        GameOver
    }
    public partial class Form1 : Form
    {
        BattleShip player1;
        BattleShip player2;
        Timer timer = new Timer();
        List<Projectile> projectiles = new List<Projectile>();
        State state = State.BeforeStart;
        string FinalText = "";
        public Form1()
        {
            AllocConsole();
            InitializeComponent();
            player1 = new BattleShip(this, Constants.Ship.PlayerShip.Player1, Constants.FormWidth, Constants.FormHeight);
            player2 = new BattleShip(this, Constants.Ship.PlayerShip.Player2, Constants.FormWidth, Constants.FormHeight);
            this.ClientSize = new Size(Constants.FormWidth, Constants.FormHeight);
            this.ResizeRedraw = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            this.Paint += new PaintEventHandler(this.Form1_Paint);
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

        internal void RemoveProjectile(Projectile projectile)
        {
            Controls.Remove(projectile);
            projectile.toRemove = true;
        }

        public void CheckCollision(Rectangle projectile)
        {
            if (player1.Bounds.IntersectsWith(projectile))
            {
                Console.WriteLine("player 1 has been shot");
                FinalText = "Player 2 won!";
                state = State.GameOver;
                Invalidate();
            } else if (player2.Bounds.IntersectsWith(projectile))
            {
                Console.WriteLine("player 2 has been shot");
                FinalText = "Player 1 won!";
                state = State.GameOver;
                Invalidate();
            }
        }

        public bool HasObstacle(Rectangle rectangle)
        {
            var region1 = player1.Bounds;
            var region2 = player2.Bounds;
            return region1.IntersectsWith(rectangle) || region2.IntersectsWith(rectangle);
        }

        private void InitializeShip(BattleShip ship, Point location)
        {
            ship.Location = location;
            Controls.Add(ship);
        }

        private void Form1_Load(object sender, EventArgs e) {}

        private void StepTimer(object sender, EventArgs e)
        {
            switch (state)
            {
                case State.BeforeStart:
                    this.StepTimerBeforeStart(sender, e);
                    break;
                case State.InProgress:
                    this.StepTimerInProgress(sender, e);
                    break;
                case State.GameOver:
                    this.StepTimerGameOver(sender, e);
                    break;
            }
        }

        private void StepTimerBeforeStart(object sender, EventArgs e) {

        }

        private void StepTimerInProgress(object sender, EventArgs e) {
            player1.MoveShip();
            player2.MoveShip();
            foreach (var projectile in projectiles)
            {
                projectile.MoveProjectile();
            }
            var list = projectiles.Where(x => x.toRemove).ToList();
            foreach (var projectile in list)
            {
                projectiles.Remove(projectile);
                Controls.Remove(projectile);
                projectile.Dispose();
            }
        }

        private void StepTimerGameOver(object sender, EventArgs e) {}

        public void CreateProjectile(Constants.Ship.PlayerShip playerShip) {
            var projectile = new Projectile(this, playerShip, this.ClientSize.Width, this.ClientSize.Height);
            BattleShip trigger;
            if (playerShip == Constants.Ship.PlayerShip.Player1) trigger = player1; else trigger = player2;
            Point location = trigger.Location;
            location.X += trigger.Width * projectile.DirectionValue;
            projectile.Location = location;
            projectiles.Add(projectile);
            Controls.Add(projectile);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("key: " + e.KeyCode);
            switch (state)
            {
                case State.BeforeStart:
                    this.Form1_KeyDownBeforeStart(sender, e);
                    break;
                case State.InProgress:
                    this.Form1_KeyDownInProgress(sender, e);
                    break;
                case State.GameOver:
                    this.Form1_KeyDownGameOver(sender, e);
                    break;
            }
        }

        private void Form1_KeyDownBeforeStart(object sender, KeyEventArgs e) {
            state = State.InProgress;
            Reset();
            Invalidate();
        }

        private void Reset()
        {
            FinalText = "";
            player1.Location = new Point(0, this.ClientSize.Height / 2);
            player2.Location = new Point(this.ClientSize.Width - player1.Width, this.ClientSize.Height / 2);
            foreach (var projectile in projectiles)
            {
                Controls.Remove(projectile);
            }
            projectiles.Clear();
            Invalidate();
        }

        private void Form1_KeyDownInProgress(object sender, KeyEventArgs e) {
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
                case Constants.Player1Controls.Fire:
                    CreateProjectile(Constants.Ship.PlayerShip.Player1);
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
                case Constants.Player2Controls.Fire:
                    CreateProjectile(Constants.Ship.PlayerShip.Player2);
                    break;
            }
        }

        private void Form1_KeyDownGameOver(object sender, KeyEventArgs e) {
            state = State.BeforeStart;
            Reset();
            Invalidate();
        }
    
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            switch (state)
            {
                case State.BeforeStart:
                    this.Form1_PaintBeforeStart(sender, e);
                    break;
                case State.InProgress:
                    this.Form1_PaintInProgress(sender, e);
                    break;
                case State.GameOver:
                    this.Form1_PaintGameOver(sender, e);
                    break;
            }
        }

        private void Form1_PaintBeforeStart(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.DrawString("Нажмите любую клавишу чтобы начать", new Font("Arial", 20), Brushes.Black, new Point(this.ClientSize.Width / 2 - 250, 0));
        }

        private void Form1_PaintInProgress(object sender, PaintEventArgs e)
        {
        }

        private void Form1_PaintGameOver(object sender, PaintEventArgs e)   
        {
            var graphics = e.Graphics;
            graphics.DrawString(FinalText, new Font("Arial", 20), Brushes.Black, new Point(this.ClientSize.Width / 2 - 100, 0));
        }
    }
}
