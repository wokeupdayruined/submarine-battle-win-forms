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
    }

    public static class Player2Controls
    {
        public const Keys MoveRight = Keys.L;
        public const Keys MoveDown = Keys.K;
        public const Keys MoveLeft = Keys.J;
        public const Keys MoveUp = Keys.I;
    }

    public static class Ship
    {
        public const int Width = 40;
        public const int Height = 40;
    }

  }
}
