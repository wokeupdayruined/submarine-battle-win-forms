using System;

namespace sea_battle_C_
{
    public class GameSettings
    {
        public int StartHealth { get; set; } = 100;
        public float ShipSpeed { get; set; } = 10f;
        public double PowerUpSpawnIntervalSeconds { get; set; } = 10.0;
        public float Friction { get; set; } = 0.1f;
        public float Acceleration { get; set; } = 1.0f;
        public double OrdinaryFireCooldownSeconds { get; set; } = 1.0;
        public double HomingFireCooldownSeconds { get; set; } = 5.0;
        public double MineFireCooldownSeconds { get; set; } = 2.0;
        public int OrdinaryDamage { get; set; } = 50;
        public int HomingDamage { get; set; } = 30;
        public int MineDamage { get; set; } = 80;
        public float PowerUpSpeedMultiplier { get; set; } = 1.5f;
        public int PowerUpHealthBoost { get; set; } = 30;
        public float PowerUpFireRateMultiplier { get; set; } = 0.5f;

        public void ResetToDefaults()
        {
            StartHealth = 100;
            ShipSpeed = 10f;
            PowerUpSpawnIntervalSeconds = 10.0;
            Friction = 0.1f;
            Acceleration = 1.0f;
            OrdinaryFireCooldownSeconds = 1.0;
            HomingFireCooldownSeconds = 5.0;
            MineFireCooldownSeconds = 2.0;
            OrdinaryDamage = 50;
            HomingDamage = 50;
            MineDamage = 80;
            PowerUpSpeedMultiplier = 1.5f;
            PowerUpHealthBoost = 30;
            PowerUpFireRateMultiplier = 0.5f;
        }
    }
}