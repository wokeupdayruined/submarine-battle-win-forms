using System.Windows.Forms;

namespace sea_battle_C_
{
  public static class Constants
  {
    public static int FormWidth = 800;
    public static int FormHeight = 600;
    public static class Player1Controls
    {
        public const Keys MoveRight = Keys.D;
        public const Keys MoveDown = Keys.S;
        public const Keys MoveLeft = Keys.A;
        public const Keys MoveUp = Keys.W;
        public const Keys Fire = Keys.E;
    }

    public static class Player2Controls
    {
        public const Keys MoveRight = Keys.L;
        public const Keys MoveDown = Keys.K;
        public const Keys MoveLeft = Keys.J;
        public const Keys MoveUp = Keys.I;
        public const Keys Fire = Keys.O;
    }

    public static class Ship
    {
        public enum PlayerShip {
            Player1 = 1,
            Player2
        }
        public const string ImagePath1 = "resources/ship1.png";
        public const string ImagePath2 = "resources/ship2.png";
        public const string ProjectilePath1 = "resources/projectile1.png";
        public const string ProjectilePath2 = "resources/projectile2.png";
        public const int Width = 80;
        public const int Height = 40;
    }

  }
}
