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
    enum FacingDirection {Left, Right};
    internal class BattleShip : Control
    {
        Bitmap Bitmap { get; set; }
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int XSpeed { get; set; } = 5;
        public int YSpeed { get; set; } = 5;
        public Direction Direction { get; set; } = Direction.None;
        public BattleShip(FacingDirection direction)
        {
            Bitmap = new Bitmap("resources/ship.jpg");
            Bitmap = new Bitmap(Bitmap, new Size(Constants.Ship.Width, Constants.Ship.Height));
            Width = Constants.Ship.Width;
            Height = Constants.Ship.Height;
            if (direction == FacingDirection.Right)
                Bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
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
            base.OnPaint(e);
        }
    }
}
