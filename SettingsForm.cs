using System;
using System.Windows.Forms;

namespace sea_battle_C_
{
    public partial class SettingsForm : Form
    {
        private GameSettings settings;
        private TextBox txtStartHealth;
        private TextBox txtShipSpeed;
        private TextBox txtPowerUpSpawnInterval;
        private TextBox txtFriction;
        private TextBox txtAcceleration;
        private TextBox txtOrdinaryCooldown;
        private TextBox txtHomingCooldown;
        private TextBox txtMineCooldown;
        private TextBox txtOrdinaryDamage;
        private TextBox txtHomingDamage;
        private TextBox txtMineDamage;
        private TextBox txtPowerUpSpeedMultiplier;
        private TextBox txtPowerUpHealthBoost;
        private TextBox txtPowerUpFireRateMultiplier;
        private Button btnApply;
        private Button btnReset;

        public SettingsForm(GameSettings settings)
        {
            this.settings = settings;
            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "Настройки игры";
            this.Size = new System.Drawing.Size(400, 600);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            int labelWidth = 200;
            int textBoxWidth = 150;

            // Создаём поля ввода
            AddSettingField("Начальное здоровье:", ref txtStartHealth, ref y, labelWidth, textBoxWidth);
            AddSettingField("Скорость кораблей:", ref txtShipSpeed, ref y, labelWidth, textBoxWidth);
            AddSettingField("Интервал спавна бонусов (с):", ref txtPowerUpSpawnInterval, ref y, labelWidth, textBoxWidth);
            AddSettingField("Трение:", ref txtFriction, ref y, labelWidth, textBoxWidth);
            AddSettingField("Ускорение:", ref txtAcceleration, ref y, labelWidth, textBoxWidth);
            AddSettingField("Кулдаун обычных торпед (с):", ref txtOrdinaryCooldown, ref y, labelWidth, textBoxWidth);
            AddSettingField("Кулдаун самонаводящихся (с):", ref txtHomingCooldown, ref y, labelWidth, textBoxWidth);
            AddSettingField("Кулдаун мин (с):", ref txtMineCooldown, ref y, labelWidth, textBoxWidth);
            AddSettingField("Урон обычных торпед:", ref txtOrdinaryDamage, ref y, labelWidth, textBoxWidth);
            AddSettingField("Урон самонаводящихся:", ref txtHomingDamage, ref y, labelWidth, textBoxWidth);
            AddSettingField("Урон мин:", ref txtMineDamage, ref y, labelWidth, textBoxWidth);
            AddSettingField("Множитель скорости бонуса:", ref txtPowerUpSpeedMultiplier, ref y, labelWidth, textBoxWidth);
            AddSettingField("Здоровье от бонуса:", ref txtPowerUpHealthBoost, ref y, labelWidth, textBoxWidth);
            AddSettingField("Множитель кулдауна бонуса:", ref txtPowerUpFireRateMultiplier, ref y, labelWidth, textBoxWidth);

            // Кнопки
            btnApply = new Button { Text = "Применить", Location = new System.Drawing.Point(50, y + 20), Size = new System.Drawing.Size(100, 30) };
            btnReset = new Button { Text = "По умолчанию", Location = new System.Drawing.Point(200, y + 20), Size = new System.Drawing.Size(100, 30) };
            btnApply.Click += BtnApply_Click;
            btnReset.Click += BtnReset_Click;

            this.Controls.Add(btnApply);
            this.Controls.Add(btnReset);
        }

        private void AddSettingField(string labelText, ref TextBox textBox, ref int y, int labelWidth, int textBoxWidth)
        {
            Label label = new Label { Text = labelText, Location = new System.Drawing.Point(20, y), Size = new System.Drawing.Size(labelWidth, 20) };
            textBox = new TextBox { Location = new System.Drawing.Point(20 + labelWidth, y), Size = new System.Drawing.Size(textBoxWidth, 20) };
            this.Controls.Add(label);
            this.Controls.Add(textBox);
            y += 30;
        }

        private void LoadSettings()
        {
            txtStartHealth.Text = settings.StartHealth.ToString();
            txtShipSpeed.Text = settings.ShipSpeed.ToString();
            txtPowerUpSpawnInterval.Text = settings.PowerUpSpawnIntervalSeconds.ToString();
            txtFriction.Text = settings.Friction.ToString();
            txtAcceleration.Text = settings.Acceleration.ToString();
            txtOrdinaryCooldown.Text = settings.OrdinaryFireCooldownSeconds.ToString();
            txtHomingCooldown.Text = settings.HomingFireCooldownSeconds.ToString();
            txtMineCooldown.Text = settings.MineFireCooldownSeconds.ToString();
            txtOrdinaryDamage.Text = settings.OrdinaryDamage.ToString();
            txtHomingDamage.Text = settings.HomingDamage.ToString();
            txtMineDamage.Text = settings.MineDamage.ToString();
            txtPowerUpSpeedMultiplier.Text = settings.PowerUpSpeedMultiplier.ToString();
            txtPowerUpHealthBoost.Text = settings.PowerUpHealthBoost.ToString();
            txtPowerUpFireRateMultiplier.Text = settings.PowerUpFireRateMultiplier.ToString();
        }

        private void BtnApply_Click(object sender, EventArgs e)
        {
            try
            {
                settings.StartHealth = int.Parse(txtStartHealth.Text);
                settings.ShipSpeed = float.Parse(txtShipSpeed.Text);
                settings.PowerUpSpawnIntervalSeconds = double.Parse(txtPowerUpSpawnInterval.Text);
                settings.Friction = float.Parse(txtFriction.Text);
                settings.Acceleration = float.Parse(txtAcceleration.Text);
                settings.OrdinaryFireCooldownSeconds = double.Parse(txtOrdinaryCooldown.Text);
                settings.HomingFireCooldownSeconds = double.Parse(txtHomingCooldown.Text);
                settings.MineFireCooldownSeconds = double.Parse(txtMineCooldown.Text);
                settings.OrdinaryDamage = int.Parse(txtOrdinaryDamage.Text);
                settings.HomingDamage = int.Parse(txtHomingDamage.Text);
                settings.MineDamage = int.Parse(txtMineDamage.Text);
                settings.PowerUpSpeedMultiplier = float.Parse(txtPowerUpSpeedMultiplier.Text);
                settings.PowerUpHealthBoost = int.Parse(txtPowerUpHealthBoost.Text);
                settings.PowerUpFireRateMultiplier = float.Parse(txtPowerUpFireRateMultiplier.Text);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка ввода: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            settings.ResetToDefaults();
            LoadSettings();
        }
    }
}