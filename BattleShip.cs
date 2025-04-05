using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sea_battle_C_
{
    enum Direction { Right, Left, Down, Up, None };
    internal class BattleShip : Control
    {
        private Bitmap bitmap;
        public ref Bitmap Bitmap
        {
            get { return ref bitmap; }
        }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int XSpeed { get; set; } = 5;
        public int YSpeed { get; set; } = 5;
        public Direction Direction { get; set; } = Direction.None;
        public int Health { get; set; } = Constants.Ship.Health;
        private List<WeakReference<Projectile>> projectiles = new List<WeakReference<Projectile>>();
        public Rectangle ShipBounds => new Rectangle(Location.X, Location.Y, Bitmap.Width, Bitmap.Height);
        public Label label = new Label();
        Form1 Form { get; set; }
        public BattleShip(Form1 form, Constants.Ship.PlayerShip playerShip, int maxX, int maxY)
        {
            DoubleBuffered = true;
            Form = form;
            if (playerShip == Constants.Ship.PlayerShip.Player1)
            {
                Bitmap = new Bitmap(Image.FromFile(Constants.Ship.ImagePath1), new Size(Constants.Ship.Width, Constants.Ship.Height));
            } else {
                Bitmap = new Bitmap(Image.FromFile(Constants.Ship.ImagePath2), new Size(Constants.Ship.Width, Constants.Ship.Height));
            }
            Width = Constants.Ship.Width;
            Height = Constants.Ship.Height + 20;
            MaxX = maxX;
            MaxY = maxY;
            this.Controls.Add(label);
            label.Text = Health.ToString();
            label.Location = new Point(0, Bitmap.Height + 2);
            label.Font = new Font("Arial", 14);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void UpClicked()
        {
            switch (this.Direction)
            {
                case Direction.Down:
                    Direction = Direction.None;
                    break;
                default:
                    Direction = Direction.Up;
                    break;
            }
        }

        public void DownClicked()
        {
            switch (this.Direction)
            {
                case Direction.Up:
                    Direction = Direction.None;
                    break;
                default:
                    Direction = Direction.Down;
                    break;
            }
        }

        public void RightClicked()
        {
            switch (this.Direction)
            {
                case Direction.Left:
                    Direction = Direction.None;
                    break;
                default:
                    Direction = Direction.Right;
                    break;
            }
        }

        public void LeftClicked()
        {
            switch (this.Direction)
            {
                case Direction.Right:
                    Direction = Direction.None;
                    break;
                default:
                    Direction = Direction.Left;
                    break;
            }
        }

        public void MoveShip()
        {
            switch (Direction)
            {
                case Direction.Right:
                    MoveRight();
                    break;
                case Direction.Left:
                    MoveLeft();
                    break;
                case Direction.Down:
                    MoveDown();
                    break;
                case Direction.Up:
                    MoveUp();
                    break;
                default:
                    break;
            }
        }

        public void MoveRight()
        {
            var nextLocation = Location;
            nextLocation.X = Math.Min(nextLocation.X + XSpeed, MaxX - Width);
            var stepRectangle = new Rectangle(Location.X + Width, Location.Y, XSpeed, Height);
            if (Form.HasObstacle(stepRectangle))
                return;
            Location = nextLocation;
        }

        public void MoveLeft()
        {
            var location = Location;
            location.X = Math.Max(location.X - XSpeed, 0);
            var stepRectangle = new Rectangle(Location.X - XSpeed, Location.Y, XSpeed, Height);
            if (Form.HasObstacle(stepRectangle))
                return;
            Location = location;
        }

        public void MoveDown()
        {
            var location = Location;
            location.Y = Math.Min(location.Y + YSpeed, MaxY - Height);
            var stepRectangle = new Rectangle(Location.X, Location.Y + Height, Width, YSpeed);
            if (Form.HasObstacle(stepRectangle))
                return;
            Location = location;
        }

        public void MoveUp()
        {
            var location = Location;
            location.Y = Math.Max(location.Y - YSpeed, 0);
            var stepRectangle = new Rectangle(Location.X, Location.Y - YSpeed, Width, YSpeed);
            if (Form.HasObstacle(stepRectangle))
                return;
            Location = location;
        }

        public void Hit(Projectile projectile) {

            for (int i = 0; i < projectiles.Count; i++) {
                if (projectiles[i].TryGetTarget(out var target) && target == projectile) {
                    return;
                }
            }

            this.projectiles.Add(new WeakReference<Projectile>(projectile));
            this.Health -= projectile.Damage;
            if (this.Health < 0) {
                this.Health = 0;
            }
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
            base.OnPaint(e);
        }

        public void Reinitialize() {
            this.Health = Constants.Ship.Health;
            this.Direction = Direction.None;
        }
    }
}
