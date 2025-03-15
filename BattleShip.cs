using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sea_battle_C_
{
    enum Direction { Right, Left, Down, Up };
    internal class BattleShip : Control
    {
        Bitmap Bitmap { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int XSpeed { get; set; } = 5;
        public int YSpeed { get; set; } = 5;
        public Direction Direction { get; set; }
        public BattleShip()
        {
            Bitmap = new Bitmap("resources/ship.jpg");
            Bitmap = new Bitmap(Bitmap, new Size(40, 40));
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
            }
        }

        public void MoveRight()
        {
            var location = Location;
            location.X = Math.Min(location.X + XSpeed, MaxX - Width);
            Location = location;
        }

        public void MoveLeft()
        {
            var location = Location;
            location.X = Math.Max(location.X - XSpeed, 0);
            Location = location;
        }

        public void MoveDown()
        {
            var location = Location;
            location.Y = Math.Min(location.Y + YSpeed, MaxY - Height);
            Location = location;
        }

        public void MoveUp()
        {
            var location = Location;
            location.Y = Math.Max(location.Y - YSpeed, 0);
            Location = location;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
        }
    }
}
