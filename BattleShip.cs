using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sea_battle_C_
{
    // Класс BattleShip представляет подводную лодку игрока
    public class BattleShip : Control
    {
        // Изображение лодки
        Bitmap Bitmap { get; set; }
        // Текущее здоровье лодки
        public int Health { get; set; }
        // Максимальная координата X игрового поля
        public int maxX { get; set; }
        // Максимальная координата Y игрового поля
        public int maxY { get; set; }

        // Настройки игры
        private readonly GameSettings settings;
        // Базовая скорость лодки
        private float baseSpeed;
        // Текущая скорость лодки
        public float Speed { get; set; }
        // Время последнего выстрела для каждого типа торпед
        private Dictionary<Constants.Ship.TorpedoType, DateTime> lastFireTimes;
        // Перезарядка для каждого типа торпед
        private Dictionary<Constants.Ship.TorpedoType, float> fireCooldowns;
        // Время окончания ускорения
        private DateTime? speedBoostEndTime;
        // Время окончания ускоренной перезарядки
        private DateTime? fireRateBoostEndTime;
        // Флаг движения вправо
        public bool IsMovingRight { get; set; }
        // Флаг движения влево
        public bool IsMovingLeft { get; set; }
        // Флаг движения вверх
        public bool IsMovingUp { get; set; }
        // Флаг движения вниз
        public bool IsMovingDown { get; set; }
        // Ссылка на главную форму игры
        Form1 form { get; set; }
        // Идентификатор игрока (Player1 или Player2)
        Constants.Ship.PlayerShip playerShip;
        // Текущая скорость по оси X
        private float velocityX = 0f;
        // Текущая скорость по оси Y
        private float velocityY = 0f;

        // Прямоугольник, описывающий границы лодки
        public Rectangle ShipBounds
        {
            get => new Rectangle(Location.X, Location.Y, this.Width, this.Height);
        }

        // Конструктор лодки
        public BattleShip(Form1 form, Constants.Ship.PlayerShip playerShip, int maxX, int maxY, GameSettings settings)
        {
            // Включаем двойную буферизацию для плавного отображения
            DoubleBuffered = true;
            this.form = form;
            this.playerShip = playerShip;
            this.settings = settings;
            // Выбираем изображение в зависимости от игрока
            string imagePath = playerShip == Constants.Ship.PlayerShip.Player1 ? Constants.Ship.ImagePath1 : Constants.Ship.ImagePath2;
            Console.WriteLine(imagePath);
            try
            {
                // Загружаем и масштабируем изображение лодки
                Bitmap = new Bitmap(Image.FromFile(imagePath), new Size(Constants.Ship.Width, Constants.Ship.Height));
                Console.WriteLine($"Загружено изображение для {playerShip}: Ширина={Bitmap.Width}, Высота={Bitmap.Height}");
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибку загрузки изображения
                Console.WriteLine($"Ошибка загрузки изображения {imagePath}: {ex.Message}");
                throw;
            }
            // Устанавливаем размеры лодки
            base.Width = Constants.Ship.Width;
            base.Height = Constants.Ship.Height;
            this.maxX = maxX;
            this.maxY = maxY;
            // Устанавливаем прозрачный фон
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            // Делаем лодку видимой и активной
            Visible = true;
            Enabled = true;
            // Устанавливаем начальное здоровье из настроек
            Health = settings.StartHealth;
            // Устанавливаем базовую и текущую скорость
            baseSpeed = settings.ShipSpeed;
            Speed = baseSpeed;
            // Инициализируем времена выстрелов
            lastFireTimes = new Dictionary<Constants.Ship.TorpedoType, DateTime>
            {
                { Constants.Ship.TorpedoType.Ordinary, DateTime.MinValue },
                { Constants.Ship.TorpedoType.Homing, DateTime.MinValue },
                { Constants.Ship.TorpedoType.Mine, DateTime.MinValue }
            };
            // Инициализируем перезарядки для торпед
            fireCooldowns = new Dictionary<Constants.Ship.TorpedoType, float>
            {
                { Constants.Ship.TorpedoType.Ordinary, (float)settings.OrdinaryFireCooldownSeconds },
                { Constants.Ship.TorpedoType.Homing, (float)settings.HomingFireCooldownSeconds },
                { Constants.Ship.TorpedoType.Mine, (float)settings.MineFireCooldownSeconds }
            };
            Console.WriteLine($"Лодка {playerShip} инициализирована, Видимость: {Visible}, Активность: {Enabled}, Размер: {base.Width}x{base.Height}, Позиция: ({Location.X}, {Location.Y})");
        }

        // Регистрирует время выстрела для указанного типа торпеды
        public void Fire(Constants.Ship.TorpedoType torpedoType)
        {
            lastFireTimes[torpedoType] = DateTime.Now;
        }

        // Проверяет, можно ли выстрелить указанным типом торпеды
        public bool CanFire(Constants.Ship.TorpedoType torpedoType)
        {
            return (DateTime.Now - lastFireTimes[torpedoType]).TotalSeconds >= fireCooldowns[torpedoType];
        }

        // Обновляет позицию лодки с учетом ввода и физики
        public void MoveShip()
        {
            // Обновляем состояние бонусов
            UpdatePowerUps();
            var nextLocation = Location;

            // Вычисляем желаемое ускорение на основе ввода
            float inputX = 0f, inputY = 0f;
            if (IsMovingRight) inputX += 1f;
            if (IsMovingLeft) inputX -= 1f;
            if (IsMovingUp) inputY -= 1f;
            if (IsMovingDown) inputY += 1f;

            // Нормализуем диагональное движение для равномерной скорости
            if (inputX != 0 && inputY != 0)
            {
                float length = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
                inputX /= length;
                inputY /= length;
            }

            // Применяем ускорение к текущей скорости
            velocityX += inputX * settings.Acceleration;
            velocityY += inputY * settings.Acceleration;

            // Применяем трение для замедления
            velocityX *= (1f - settings.Friction);
            velocityY *= (1f - settings.Friction);

            // Ограничиваем максимальную скорость
            float velocityMagnitude = (float)Math.Sqrt(velocityX * velocityX + velocityY * velocityY);
            if (velocityMagnitude > Speed)
            {
                float scale = Speed / velocityMagnitude;
                velocityX *= scale;
                velocityY *= scale;
            }

            // Обновляем позицию лодки
            nextLocation.X += (int)velocityX;
            nextLocation.Y += (int)velocityY;

            // Ограничиваем движение в пределах экрана и разделяем поле для игроков
            if (playerShip == Constants.Ship.PlayerShip.Player1)
            {
                nextLocation.X = Math.Max(0, Math.Min(nextLocation.X, maxX / 2 - Width));
            }
            else // Player2
            {
                nextLocation.X = Math.Max(maxX / 2, Math.Min(nextLocation.X, maxX - Width));
            }
            nextLocation.Y = Math.Max(0, Math.Min(nextLocation.Y, maxY - Height));

            // Проверяем, нет ли столкновений с другими объектами
            if (!form.HasObstacle(new Rectangle(nextLocation.X, nextLocation.Y, Width, Height), this))
                Location = nextLocation;
        }

        // Сбрасывает состояние лодки для нового матча
        public void Reinitialize()
        {
            // Восстанавливаем начальное здоровье
            Health = settings.StartHealth;
            // Восстанавливаем базовую скорость
            Speed = baseSpeed;
            // Сбрасываем перезарядки
            fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds;
            fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds;
            fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds;
            // Отключаем активные бонусы
            speedBoostEndTime = null;
            fireRateBoostEndTime = null;
            // Сбрасываем времена выстрелов
            lastFireTimes[Constants.Ship.TorpedoType.Ordinary] = DateTime.MinValue;
            lastFireTimes[Constants.Ship.TorpedoType.Homing] = DateTime.MinValue;
            lastFireTimes[Constants.Ship.TorpedoType.Mine] = DateTime.MinValue;
            // Сбрасываем скорость
            velocityX = 0f;
            velocityY = 0f;
        }

        // Обрабатывает попадание торпеды
        public void Hit(Projectile projectile)
        {
            Console.WriteLine($"Попадание в {playerShip}, урон: {projectile.Damage}, здоровье до: {Health}");
            // Уменьшаем здоровье на величину урона
            Health -= projectile.Damage;
            Console.WriteLine($"Здоровье после: {Health}");
        }

        // Применяет бонус к лодке
        public void ApplyPowerUp(PowerUp.PowerUpType type)
        {
            switch (type)
            {
                case PowerUp.PowerUpType.SpeedBoost:
                    // Увеличиваем скорость
                    Speed = baseSpeed * settings.PowerUpSpeedMultiplier;
                    speedBoostEndTime = DateTime.Now.AddSeconds(Constants.Ship.PowerUpDurationSeconds);
                    Console.WriteLine($"Применено ускорение к {playerShip}, скорость: {Speed}");
                    break;
                case PowerUp.PowerUpType.FireRateBoost:
                    // Уменьшаем время перезарядки
                    fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    fireRateBoostEndTime = DateTime.Now.AddSeconds(Constants.Ship.PowerUpDurationSeconds);
                    Console.WriteLine($"Применено ускорение перезарядки к {playerShip}, перезарядки: Обычная={fireCooldowns[Constants.Ship.TorpedoType.Ordinary]}с, Самонаводящаяся={fireCooldowns[Constants.Ship.TorpedoType.Homing]}с, Мина={fireCooldowns[Constants.Ship.TorpedoType.Mine]}с");
                    break;
                case PowerUp.PowerUpType.HealthBoost:
                    // Восстанавливаем здоровье, не превышая максимум
                    Health = Math.Min(Health + settings.PowerUpHealthBoost, settings.StartHealth);
                    Console.WriteLine($"Применено восстановление здоровья к {playerShip}, здоровье: {Health}");
                    break;
            }
        }

        // Обновляет состояние бонусов
        public void UpdatePowerUps()
        {
            // Проверяем окончание ускорения
            if (speedBoostEndTime.HasValue && DateTime.Now >= speedBoostEndTime.Value)
            {
                Speed = baseSpeed;
                speedBoostEndTime = null;
                Console.WriteLine($"Ускорение истекло для {playerShip}, скорость: {Speed}");
            }
            // Проверяем окончание ускоренной перезарядки
            if (fireRateBoostEndTime.HasValue && DateTime.Now >= fireRateBoostEndTime.Value)
            {
                fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds;
                fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds;
                fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds;
                fireRateBoostEndTime = null;
                Console.WriteLine($"Ускорение перезарядки истекло для {playerShip}, перезарядки: Обычная={fireCooldowns[Constants.Ship.TorpedoType.Ordinary]}с, Самонаводящаяся={fireCooldowns[Constants.Ship.TorpedoType.Homing]}с, Мина={fireCooldowns[Constants.Ship.TorpedoType.Mine]}с");
            }
        }

        // Отрисовывает лодку
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Console.WriteLine($"Отрисовка {playerShip} на ({Location.X}, {Location.Y})");
            // Рисуем изображение лодки
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
            base.OnPaint(e);
        }
    }
}