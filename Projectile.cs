using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sea_battle_C_
{
    public enum ProjectileDirection { Right, Left };
    public class Projectile : Control
    {
        Bitmap Bitmap { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int XSpeed { get; set; } = 5;
        public int YSpeed { get; set; } = 5;
        public ProjectileDirection Direction { get; set; } = ProjectileDirection.Right;
        Form1 Form { get; set; }
        public int DirectionValue { get; private set; } = 1;
        public bool toRemove { get; set; } = false;
        public int Damage { get; set; } = 50;
        public Projectile(Form1 form, Constants.Ship.PlayerShip playerShip, int maxX, int maxY)
        {
            DoubleBuffered = true;
            Form = form;
            if (playerShip == Constants.Ship.PlayerShip.Player1) {
                Bitmap = new Bitmap(Image.FromFile(Constants.Ship.ProjectilePath1), new Size(Constants.Ship.Width, Constants.Ship.Height));
            } else {
                Bitmap = new Bitmap(Image.FromFile(Constants.Ship.ProjectilePath2), new Size(Constants.Ship.Width, Constants.Ship.Height));
                Direction = ProjectileDirection.Left;
                DirectionValue = -1;
            }
            Width = Constants.Ship.Width;
            Height = Constants.Ship.Height;
            MaxX = maxX;
            MaxY = maxY;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        public void MoveProjectile()
        {
            switch (Direction)
            {
                case ProjectileDirection.Right:
                    MoveRight();
                    break;
                case ProjectileDirection.Left:
                    MoveLeft();
                    break;
                default:
                    break;
            }
        }

        public void MoveRight()
        {
            Console.WriteLine("Moving projectile to the right");
            var nextLocation = Location;
            nextLocation.X += XSpeed;
            Location = nextLocation;
            Form.CheckCollision(this);
            if (Location.X > MaxX)
            {
                Console.WriteLine("Projectile has left the screen");
                Form.RemoveProjectile(this);
            }
        }

        public void MoveLeft()
        {
            Console.WriteLine("Moving projectile to the left");
            var nextLocation = Location;
            nextLocation.X -= XSpeed;
            Location = nextLocation;
            Form.CheckCollision(this);
            if (Location.X < -Width)
            {
                Console.WriteLine("Projectile has left the screen");
                Form.RemoveProjectile(this);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
            base.OnPaint(e);
        }
    }
}
