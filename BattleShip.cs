using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sea_battle_C_
{
    // Класс, представляющий корабль игрока
    public class BattleShip : Control
    {
        // Кадры анимации для нормального состояния
        private Bitmap[] defaultFrames;
        // Кадры анимации для повреждённого состояния
        private Bitmap[] damagedFrames;
        // Текстура для уничтоженного состояния
        private Bitmap destroyedFrame;
        // Текущий кадр анимации
        private int currentFrame;
        // Время последней смены кадра
        private DateTime lastFrameChange;
        // Длительность одного кадра анимации (в секундах)
        private const double FrameDurationSeconds = 0.1;
        // Текущая скорость по оси X
        private float velocityX = 0f;
        // Текущая скорость по оси Y
        private float velocityY = 0f;
        // Здоровье корабля
        public int Health { get; set; }
        // Максимальная координата X для движения
        public int maxX { get; set; }
        // Максимальная координата Y для движения
        public int maxY { get; set; }
        // Настройки игры
        private readonly GameSettings settings;
        // Базовая скорость корабля
        private float baseSpeed;
        // Текущая скорость корабля
        public float Speed { get; set; }
        // Время последнего выстрела для каждого типа снаряда
        private Dictionary<Constants.Ship.TorpedoType, DateTime> lastFireTimes;
        // Кулдауны для каждого типа снаряда
        private Dictionary<Constants.Ship.TorpedoType, float> fireCooldowns;
        // Время окончания бонуса скорости
        private DateTime? speedBoostEndTime;
        // Время окончания бонуса скорострельности
        private DateTime? fireRateBoostEndTime;
        // Флаг движения вправо
        public bool IsMovingRight { get; set; }
        // Флаг движения влево
        public bool IsMovingLeft { get; set; }
        // Флаг движения вверх
        public bool IsMovingUp { get; set; }
        // Флаг движения вниз
        public bool IsMovingDown { get; set; }
        // Ссылка на главную форму
        Form1 form { get; set; }
        // Идентификатор игрока (Player1 или Player2)
        Constants.Ship.PlayerShip playerShip;

        // Хитбокс корабля
        public Rectangle ShipBounds
        {
            get => new Rectangle(Location.X, Location.Y, this.Width, this.Height);
        }

        // Конструктор корабля
        public BattleShip(Form1 form, Constants.Ship.PlayerShip playerShip, int maxX, int maxY, GameSettings settings)
        {
            // Включаем двойную буферизацию для плавной отрисовки
            DoubleBuffered = true;
            // Сохраняем ссылку на форму
            this.form = form;
            // Сохраняем идентификатор игрока
            this.playerShip = playerShip;
            // Сохраняем настройки игры
            this.settings = settings;
            // Устанавливаем начальный кадр
            currentFrame = 0;
            // Устанавливаем время последней смены кадра
            lastFrameChange = DateTime.Now;

            // Определяем базовый путь к ресурсам
            string basePath = playerShip == Constants.Ship.PlayerShip.Player1 ? "resources/player_1" : "resources/player_2";

            // Загружаем кадры анимации для нормального состояния
            defaultFrames = new Bitmap[6];
            for (int i = 0; i < 6; i++)
            {
                string imagePath = Path.Combine(basePath, "default", $"default_{i + 1}.png");
                try
                {
                    defaultFrames[i] = new Bitmap(Image.FromFile(imagePath), new Size(Constants.Ship.Width, Constants.Ship.Height));
                    Console.WriteLine($"Loaded default frame {i + 1} for {playerShip}: {imagePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading default frame {imagePath}: {ex.Message}");
                    throw;
                }
            }

            // Загружаем кадры анимации для повреждённого состояния
            damagedFrames = new Bitmap[6];
            for (int i = 0; i < 6; i++)
            {
                string imagePath = Path.Combine(basePath, "damaged", $"damaged_{i + 1}.png");
                try
                {
                    damagedFrames[i] = new Bitmap(Image.FromFile(imagePath), new Size(Constants.Ship.Width, Constants.Ship.Height));
                    Console.WriteLine($"Loaded damaged frame {i + 1} for {playerShip}: {imagePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading damaged frame {imagePath}: {ex.Message}");
                    throw;
                }
            }

            // Загружаем текстуру уничтоженного состояния
            string destroyedPath = Path.Combine(basePath, "destroyed.png");
            try
            {
                destroyedFrame = new Bitmap(Image.FromFile(destroyedPath), new Size(Constants.Ship.Width, Constants.Ship.Height));
                Console.WriteLine($"Loaded destroyed frame for {playerShip}: {destroyedPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading destroyed frame {destroyedPath}: {ex.Message}");
                throw;
            }

            // Устанавливаем размер корабля
            base.Width = Constants.Ship.Width;
            base.Height = Constants.Ship.Height;
            // Устанавливаем границы движения
            this.maxX = maxX;
            this.maxY = maxY;
            // Включаем поддержку прозрачного фона
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            // Устанавливаем прозрачный фон
            BackColor = Color.Transparent;
            // Делаем корабль видимым
            Visible = true;
            // Включаем обработку событий
            Enabled = true;
            // Устанавливаем начальное здоровье
            Health = settings.StartHealth;
            // Устанавливаем базовую скорость
            baseSpeed = settings.ShipSpeed;
            // Устанавливаем текущую скорость
            Speed = baseSpeed;
            // Инициализируем время последнего выстрела
            lastFireTimes = new Dictionary<Constants.Ship.TorpedoType, DateTime>
            {
                { Constants.Ship.TorpedoType.Ordinary, DateTime.MinValue },
                { Constants.Ship.TorpedoType.Homing, DateTime.MinValue },
                { Constants.Ship.TorpedoType.Mine, DateTime.MinValue }
            };
            // Инициализируем кулдауны выстрелов
            fireCooldowns = new Dictionary<Constants.Ship.TorpedoType, float>
            {
                { Constants.Ship.TorpedoType.Ordinary, (float)settings.OrdinaryFireCooldownSeconds },
                { Constants.Ship.TorpedoType.Homing, (float)settings.HomingFireCooldownSeconds },
                { Constants.Ship.TorpedoType.Mine, (float)settings.MineFireCooldownSeconds }
            };
            Console.WriteLine($"BattleShip {playerShip} initialized, Visible: {Visible}, Enabled: {Enabled}, Size: {base.Width}x{base.Height}, Location: ({Location.X}, {Location.Y})");
        }

        // Регистрация выстрела
        public void Fire(Constants.Ship.TorpedoType torpedoType)
        {
            // Обновляем время выстрела для указанного типа снаряда
            lastFireTimes[torpedoType] = DateTime.Now;
        }

        // Проверка возможности выстрела
        public bool CanFire(Constants.Ship.TorpedoType torpedoType)
        {
            // Проверяем, истёк ли кулдаун для указанного типа снаряда
            return (DateTime.Now - lastFireTimes[torpedoType]).TotalSeconds >= fireCooldowns[torpedoType];
        }

        // Движение корабля
        public void MoveShip()
        {
            // Обновляем состояние бонусов
            UpdatePowerUps();
            // Текущая позиция корабля
            var nextLocation = Location;

            // Вычисляем желаемое ускорение на основе ввода
            float inputX = 0f, inputY = 0f;
            if (IsMovingRight) inputX += 1f;
            if (IsMovingLeft) inputX -= 1f;
            if (IsMovingUp) inputY -= 1f;
            if (IsMovingDown) inputY += 1f;

            // Нормализуем диагональное движение
            if (inputX != 0 && inputY != 0)
            {
                float length = (float)Math.Sqrt(inputX * inputX + inputY * inputY);
                inputX /= length;
                inputY /= length;
            }

            // Применяем ускорение
            velocityX += inputX * settings.Acceleration;
            velocityY += inputY * settings.Acceleration;

            // Применяем трение (замедление)
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

            // Обновляем позицию корабля
            nextLocation.X += (int)velocityX;
            nextLocation.Y += (int)velocityY;

            // Ограничиваем движение границами экрана и половиной поля
            if (playerShip == Constants.Ship.PlayerShip.Player1)
            {
                // Ограничиваем движение первого игрока левой половиной экрана
                nextLocation.X = Math.Max(0, Math.Min(nextLocation.X, maxX / 2 - Width));
            }
            else // Player2
            {
                // Ограничиваем движение второго игрока правой половиной экрана
                nextLocation.X = Math.Max(maxX / 2, Math.Min(nextLocation.X, maxX - Width));
            }
            // Ограничиваем движение по вертикали
            nextLocation.Y = Math.Max(0, Math.Min(nextLocation.Y, maxY - Height));

            // Проверяем столкновения с другим кораблём
            if (!form.HasObstacle(new Rectangle(nextLocation.X, nextLocation.Y, Width, Height), this))
                // Обновляем позицию, если нет препятствий
                Location = nextLocation;

            // Обновляем кадр анимации
            UpdateAnimation();
        }

        // Сброс состояния корабля
        public void Reinitialize()
        {
            // Восстанавливаем здоровье
            Health = settings.StartHealth;
            // Восстанавливаем базовую скорость
            Speed = baseSpeed;
            // Восстанавливаем кулдауны
            fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds;
            fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds;
            fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds;
            // Сбрасываем бонусы
            speedBoostEndTime = null;
            fireRateBoostEndTime = null;
            // Сбрасываем время выстрелов
            lastFireTimes[Constants.Ship.TorpedoType.Ordinary] = DateTime.MinValue;
            lastFireTimes[Constants.Ship.TorpedoType.Homing] = DateTime.MinValue;
            lastFireTimes[Constants.Ship.TorpedoType.Mine] = DateTime.MinValue;
            // Сбрасываем скорость
            velocityX = 0f;
            velocityY = 0f;
            // Сбрасываем анимацию
            currentFrame = 0;
            lastFrameChange = DateTime.Now;
        }

        // Обработка попадания снаряда
        public void Hit(Projectile projectile)
        {
            Console.WriteLine($"Hit {playerShip}, damage: {projectile.Damage}, health before: {Health}");
            // Уменьшаем здоровье на величину урона
            Health -= projectile.Damage;
            Console.WriteLine($"Health after: {Health}");
            // Перерисовываем для обновления анимации
            Invalidate();
        }

        // Применение бонуса
        public void ApplyPowerUp(PowerUp.PowerUpType type)
        {
            switch (type)
            {
                case PowerUp.PowerUpType.SpeedBoost:
                    // Увеличиваем скорость
                    Speed = baseSpeed * settings.PowerUpSpeedMultiplier;
                    // Устанавливаем время действия бонуса
                    speedBoostEndTime = DateTime.Now.AddSeconds(Constants.Ship.PowerUpDurationSeconds);
                    Console.WriteLine($"Applied speed boost to {playerShip}, speed: {Speed}");
                    break;
                case PowerUp.PowerUpType.FireRateBoost:
                    // Уменьшаем кулдауны выстрелов
                    fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds * settings.PowerUpFireRateMultiplier;
                    // Устанавливаем время действия бонуса
                    fireRateBoostEndTime = DateTime.Now.AddSeconds(Constants.Ship.PowerUpDurationSeconds);
                    Console.WriteLine($"Applied fire rate boost to {playerShip}, cooldowns: Ordinary={fireCooldowns[Constants.Ship.TorpedoType.Ordinary]}s, Homing={fireCooldowns[Constants.Ship.TorpedoType.Homing]}s, Mine={fireCooldowns[Constants.Ship.TorpedoType.Mine]}s");
                    break;
                case PowerUp.PowerUpType.HealthBoost:
                    // Восстанавливаем здоровье
                    Health = Math.Min(Health + settings.PowerUpHealthBoost, settings.StartHealth);
                    Console.WriteLine($"Applied health boost to {playerShip}, health: {Health}");
                    // Перерисовываем для обновления анимации
                    Invalidate();
                    break;
            }
        }

        // Обновление состояния бонусов
        public void UpdatePowerUps()
        {
            // Проверяем окончание бонуса скорости
            if (speedBoostEndTime.HasValue && DateTime.Now >= speedBoostEndTime.Value)
            {
                // Восстанавливаем базовую скорость
                Speed = baseSpeed;
                speedBoostEndTime = null;
                Console.WriteLine($"Speed boost expired for {playerShip}, speed: {Speed}");
            }
            // Проверяем окончание бонуса скорострельности
            if (fireRateBoostEndTime.HasValue && DateTime.Now >= fireRateBoostEndTime.Value)
            {
                // Восстанавливаем стандартные кулдауны
                fireCooldowns[Constants.Ship.TorpedoType.Ordinary] = (float)settings.OrdinaryFireCooldownSeconds;
                fireCooldowns[Constants.Ship.TorpedoType.Homing] = (float)settings.HomingFireCooldownSeconds;
                fireCooldowns[Constants.Ship.TorpedoType.Mine] = (float)settings.MineFireCooldownSeconds;
                fireRateBoostEndTime = null;
                Console.WriteLine($"Fire rate boost expired for {playerShip}, cooldowns: Ordinary={fireCooldowns[Constants.Ship.TorpedoType.Ordinary]}s, Homing={fireCooldowns[Constants.Ship.TorpedoType.Homing]}s, Mine={fireCooldowns[Constants.Ship.TorpedoType.Mine]}s");
            }
        }

        // Обновление анимации
        private void UpdateAnimation()
        {
            // Проверяем, пора ли сменить кадр
            if ((DateTime.Now - lastFrameChange).TotalSeconds >= FrameDurationSeconds)
            {
                // Если корабль жив, переключаем кадры
                if (Health > 30)
                {
                    currentFrame = (currentFrame + 1) % 6; // Цикл по 6 кадрам
                }
                // Обновляем время смены кадра
                lastFrameChange = DateTime.Now;
                // Запрашиваем перерисовку
                Invalidate();
            }
        }

        // Отрисовка корабля
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Console.WriteLine($"Painting {playerShip} at ({Location.X}, {Location.Y}), Health: {Health}, Frame: {currentFrame}");

            // Выбираем текстуру в зависимости от здоровья
            if (Health <= 30)
            {
                // Отрисовываем уничтоженное состояние
                graphics.DrawImageUnscaled(destroyedFrame, 0, 0);
            }
            else if (Health <= 65)
            {
                // Отрисовываем повреждённое состояние
                graphics.DrawImageUnscaled(damagedFrames[currentFrame], 0, 0);
            }
            else
            {
                // Отрисовываем нормальное состояние
                graphics.DrawImageUnscaled(defaultFrames[currentFrame], 0, 0);
            }

            base.OnPaint(e);
        }

        // Освобождение ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Освобождаем кадры анимации
                foreach (var frame in defaultFrames)
                {
                    frame?.Dispose();
                }
                foreach (var frame in damagedFrames)
                {
                    frame?.Dispose();
                }
                // Освобождаем текстуру уничтоженного состояния
                destroyedFrame?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}