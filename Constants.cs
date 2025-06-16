using System.Windows.Forms;

namespace sea_battle_C_
{
    public static class Constants
    {
        public static int FormWidth = 960;
        public static int FormHeight = 640;
        public static string BackgroundImagePath = "resources/KrustyKrab.jpg";
        public static class Player1Controls
        {
            public const Keys MoveRight = Keys.D;
            public const Keys MoveDown = Keys.S;
            public const Keys MoveLeft = Keys.A;
            public const Keys MoveUp = Keys.W;
            public const Keys Fire = Keys.E;
            public const Keys FireHoming = Keys.Q;
            public const Keys FireMine = Keys.R;
        }

        public static class Player2Controls
        {
            public const Keys MoveRight = Keys.L;
            public const Keys MoveDown = Keys.K;
            public const Keys MoveLeft = Keys.J;
            public const Keys MoveUp = Keys.I;
            public const Keys Fire = Keys.O;
            public const Keys FireHoming = Keys.U;
            public const Keys FireMine = Keys.P;
        }

        public static class Ship
        {
            public enum PlayerShip
            {
                Player1 = 1,
                Player2
            }
            public enum TorpedoType
            {
                Ordinary,
                Homing,
                Mine
            }

            public const string ProjectilePath1 = "resources/projectile1.png";
            public const string ProjectilePath2 = "resources/projectile2.png";
            public const string HomingProjectilePath1 = "resources/homing_projectile1.png";
            public const string HomingProjectilePath2 = "resources/homing_projectile2.png";
            public const string MineProjectilePath = "resources/mine.png";
            public static readonly string[] ExplosionFramePaths = {
                   "resources/explosion/explosion1.png",
                   "resources/explosion/explosion2.png",
                   "resources/explosion/explosion3.png"
               };
            public const string FireSoundPath = "resources/fire.wav";
            public const string ExplosionSoundPath = "resources/explosion.wav";
            public const string BackgroundMusicPath = "resources/background.wav";
            public const string PowerUpSoundPath = "resources/powerup.wav";
            public static readonly string[] PowerUpImagePaths = {
                   "resources/powerup/powerup_speed.png",
                   "resources/powerup/powerup_fire.png",
                   "resources/powerup/powerup_health.png"
               };
            public const int BackgroundMusicVolume = 20;
            public const int Width = 90;
            public const int Height = 60;
            public const int ExplosionSize = 180;
            public const int PowerUpSize = 50;
            public const int ProjectileWidth = 64;
            public const int ProjectileHeight = 32;
            public const int HomingProjectileWidth = 63; 
            public const int HomingProjectileHeight = 21; 
            public const int MineSize = 60; 
            public const int MineExplosionRadius = 180; 
            public const double PowerUpDurationSeconds = 10.0;

            public const double MineDelaySeconds = 1.2;
            public const double ExplosionDurationSeconds = 0.5;
            public const double HomingProjectileLifetimeSeconds = 10.0;
        }
    }
}