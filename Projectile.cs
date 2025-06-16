using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sea_battle_C_
{
    // Класс, представляющий снаряд (торпеду или мину)
    public class Projectile : Control
    {
        // Текстура снаряда
        private Bitmap Bitmap { get; set; }
        // Тип снаряда (обычная торпеда, самонаводящаяся или мина)
        public Constants.Ship.TorpedoType TorpedoType { get; private set; }
        // Игрок, выпустивший снаряд
        public Constants.Ship.PlayerShip PlayerShip { get; private set; }
        // Ссылка на главную форму
        private Form1 Form { get; set; }
        // Настройки игры
        private readonly GameSettings settings;
        // Время создания снаряда
        private DateTime SpawnTime { get; set; }
        // Флаг для удаления снаряда
        public bool toRemove { get; set; } = false;
        // Максимальная координата X для движения
        private int maxX { get; set; }
        // Максимальная координата Y для движения
        private int maxY { get; set; }
        // Скорость снаряда
        private float Speed { get; set; } = 15f; // Начальная скорость для мин и торпед
        // Направление движения (1 для Player1, -1 для Player2)
        private float verticalVelocity = 0f; // Вертикальная скорость для самонаводящихся торпед
        public int DirectionValue => PlayerShip == Constants.Ship.PlayerShip.Player1 ? 1 : -1;
        // Целевая позиция для самонаводящихся торпед
        private Point TargetLocation { get; set; }
        // Урон, наносимый снарядом
        public int Damage { get; private set; }
        // Флаг остановки мины
        private bool isMineStopped = false;
        // Время остановки мины
        private DateTime? mineStopTime = null;
        // Начальная позиция мины для отслеживания расстояния
        private Point initialLocation;

        // Конструктор снаряда
        public Projectile(Form1 form, Constants.Ship.PlayerShip playerShip, Constants.Ship.TorpedoType torpedoType, int maxX, int maxY, GameSettings settings)
        {
            // Включаем двойную буферизацию для плавной отрисовки
            DoubleBuffered = true;
            // Сохраняем ссылку на форму
            Form = form;
            // Сохраняем игрока
            PlayerShip = playerShip;
            // Сохраняем тип снаряда
            TorpedoType = torpedoType;
            // Устанавливаем границы движения
            this.maxX = maxX;
            this.maxY = maxY;
            // Сохраняем настройки
            this.settings = settings;
            // Запоминаем время создания
            SpawnTime = DateTime.Now;
            // Определяем параметры снаряда
            string imagePath;
            int width, height;
            switch (torpedoType)
            {
                case Constants.Ship.TorpedoType.Ordinary:
                    // Обычная торпеда
                    imagePath = playerShip == Constants.Ship.PlayerShip.Player1 ? Constants.Ship.ProjectilePath1 : Constants.Ship.ProjectilePath2;
                    Damage = settings.OrdinaryDamage;
                    width = Constants.Ship.ProjectileWidth;
                    height = Constants.Ship.ProjectileHeight;
                    break;
                case Constants.Ship.TorpedoType.Homing:
                    // Самонаводящаяся торпеда
                    imagePath = playerShip == Constants.Ship.PlayerShip.Player1 ? Constants.Ship.HomingProjectilePath1 : Constants.Ship.HomingProjectilePath2;
                    Damage = settings.HomingDamage;
                    width = Constants.Ship.HomingProjectileWidth; // Используем новые константы
                    height = Constants.Ship.HomingProjectileHeight;
                    break;
                case Constants.Ship.TorpedoType.Mine:
                    // Мина
                    imagePath = Constants.Ship.MineProjectilePath;
                    Damage = settings.MineDamage;
                    width = Constants.Ship.MineSize;
                    height = Constants.Ship.MineSize;
                    break;
                default:
                    // По умолчанию — мина
                    imagePath = Constants.Ship.MineProjectilePath;
                    Damage = settings.MineDamage;
                    width = Constants.Ship.MineSize;
                    height = Constants.Ship.MineSize;
                    break;
            }
            Console.WriteLine($"Loading projectile image: {imagePath}, Damage: {Damage}");
            // Загружаем текстуру снаряда
            try
            {
                using (var originalImage = Image.FromFile(imagePath))
                {
                    Bitmap = new Bitmap(originalImage, new Size(width, height));
                    using (var graphics = Graphics.FromImage(Bitmap))
                    {
                        // Настраиваем качество масштабирования
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    Console.WriteLine($"Loaded projectile image: Width={Bitmap.Width}, Height={Bitmap.Height}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading projectile image: {ex.Message}");
                throw;
            }
            // Устанавливаем размер снаряда
            base.Width = width;
            base.Height = height;
            // Включаем прозрачный фон
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            // Определяем целевой корабль
            var targetShip = playerShip == Constants.Ship.PlayerShip.Player1 ? Form.GetPlayer2() : Form.GetPlayer1();
            // Устанавливаем целевую позицию
            TargetLocation = targetShip.Location;
            // Инициализируем начальную позицию мины
            initialLocation = Location;
        }

        // Движение снаряда
        public void MoveProjectile()
        {
            // Текущая позиция снаряда
            var nextLocation = Location;
            // Проверяем время жизни самонаводящейся торпеды
            if ((DateTime.Now - SpawnTime).TotalSeconds > Constants.Ship.HomingProjectileLifetimeSeconds && TorpedoType == Constants.Ship.TorpedoType.Homing)
            {
                Console.WriteLine($"Homing projectile lifetime expired at ({nextLocation.X}, {nextLocation.Y})");
                // Удаляем снаряд
                Form.RemoveProjectile(this);
                return;
            }
            // Обработка мины
            if (TorpedoType == Constants.Ship.TorpedoType.Mine)
            {
                if (!isMineStopped)
                {
                    // Рассчитываем пройденное расстояние
                    int distanceTraveled = Math.Abs(Location.X - initialLocation.X);
                    // Целевое расстояние — треть экрана
                    int targetDistance = maxX / 3;

                    // Плавное замедление
                    if (distanceTraveled < targetDistance)
                    {
                        // Уменьшаем скорость на 4% за кадр
                        Speed = Math.Max(0, Speed * 0.96f);
                        // Проверяем, достаточно ли мала скорость
                        if (Speed < 4f)
                        {
                            // Принудительно останавливаем мину
                            isMineStopped = true;
                            mineStopTime = DateTime.Now;
                            Speed = 0;
                            Console.WriteLine($"Mine stopped due to low speed at ({nextLocation.X}, {nextLocation.Y}), Distance: {distanceTraveled}");
                        }
                        else
                        {
                            // Двигаем мину
                            nextLocation.X += (int)(Speed * DirectionValue);
                            Console.WriteLine($"Mine moving to ({nextLocation.X}, {nextLocation.Y}), Speed: {Speed}, Distance: {distanceTraveled}");
                        }
                    }
                    else
                    {
                        // Останавливаем мину, если достигнуто целевое расстояние
                        isMineStopped = true;
                        mineStopTime = DateTime.Now;
                        Speed = 0;
                        Console.WriteLine($"Mine stopped at ({nextLocation.X}, {nextLocation.Y}), Distance: {distanceTraveled}");
                    }
                }
                // Проверяем время до взрыва
                else if (mineStopTime.HasValue && (DateTime.Now - mineStopTime.Value).TotalSeconds >= Constants.Ship.MineDelaySeconds)
                {
                    // Центрируем взрыв на мине
                    var explosionLocation = new Point(
                        Location.X + Constants.Ship.MineSize / 2,
                        Location.Y + Constants.Ship.MineSize / 2);
                    // Создаём взрыв
                    var explosion = new Explosion(Form, explosionLocation);
                    Form.AddExplosion(explosion);
                    Console.WriteLine($"Mine exploded at ({explosionLocation.X}, {explosionLocation.Y})");
                    // Проигрываем звук взрыва
                    Form.PlayExplosionSound();

                    // Создаём хитбокс взрыва
                    var explosionRect = new Rectangle(
                        Location.X - (Constants.Ship.MineExplosionRadius - Constants.Ship.MineSize) / 2,
                        Location.Y - (Constants.Ship.MineExplosionRadius - Constants.Ship.MineSize) / 2,
                        Constants.Ship.MineExplosionRadius,
                        Constants.Ship.MineExplosionRadius);

                    // Проверяем попадание по игрокам
                    var player1 = Form.GetPlayer1();
                    var player2 = Form.GetPlayer2();
                    if (player1.ShipBounds.IntersectsWith(explosionRect))
                    {
                        Console.WriteLine($"Player 1 hit by mine, damage: {Damage}");
                        // Наносим урон первому игроку
                        player1.Hit(this);
                        if (player1.Health <= 0)
                        {
                            Console.WriteLine("Player 1 destroyed by mine");
                        }
                    }
                    if (player2.ShipBounds.IntersectsWith(explosionRect))
                    {
                        Console.WriteLine($"Player 2 hit by mine, damage: {Damage}");
                        // Наносим урон второму игроку
                        player2.Hit(this);
                        if (player2.Health <= 0)
                        {
                            Console.WriteLine("Player 2 destroyed by mine");
                        }
                    }
                    // Удаляем мину
                    Form.RemoveProjectile(this);
                }
            }
            // Обработка самонаводящейся торпеды
            // Обработка самонаводящейся торпеды
            else if (TorpedoType == Constants.Ship.TorpedoType.Homing)
            {
                // Проверяем время жизни
                if ((DateTime.Now - SpawnTime).TotalSeconds > Constants.Ship.HomingProjectileLifetimeSeconds)
                {
                    Console.WriteLine($"Homing projectile lifetime expired at ({nextLocation.X}, {nextLocation.Y})");
                    Form.RemoveProjectile(this);
                    return;
                }

                // Определяем целевой корабль
                var targetShip = PlayerShip == Constants.Ship.PlayerShip.Player1 ? Form.GetPlayer2() : Form.GetPlayer1();
                // Вычисляем центр цели
                var targetLocation = new Point(
                    targetShip.Location.X + targetShip.Width / 2,
                    targetShip.Location.Y + targetShip.Height / 2);

                // Двигаем торпеду по горизонтали
                nextLocation.X += 5 * DirectionValue;

                // Проверяем, не промахнулась ли торпеда
                bool hasMissed = (PlayerShip == Constants.Ship.PlayerShip.Player1 && nextLocation.X > targetLocation.X) ||
                                 (PlayerShip == Constants.Ship.PlayerShip.Player2 && nextLocation.X < targetLocation.X);

                if (!hasMissed)
                {
                    // Вычисляем разницу по Y до цели
                    var diffY = targetLocation.Y - (nextLocation.Y + Constants.Ship.HomingProjectileHeight / 2);
                    // Определяем желаемое направление (1 вниз, -1 вверх)
                    float desiredDirection = diffY > 0 ? 1f : -1f;
                    // Ускорение по вертикали
                    float verticalAcceleration = 0.4f; // Ускорение (можно настроить)
                    // Максимальная вертикальная скорость
                    float maxVerticalSpeed = 5f; // Ограничение скорости (можно настроить)
                    // Применяем ускорение в направлении цели
                    verticalVelocity += desiredDirection * verticalAcceleration;
                    //// Применяем трение для сглаживания смены направления
                    verticalVelocity *= 0.97f; // Уменьшаем скорость на 10% для плавности
                    // Ограничиваем вертикальную скорость
                    verticalVelocity = Math.Max(-maxVerticalSpeed, Math.Min(maxVerticalSpeed, verticalVelocity));
                    // Обновляем позицию по Y
                    nextLocation.Y += (int)verticalVelocity;
                }

                Console.WriteLine($"Homing projectile moving to ({nextLocation.X}, {nextLocation.Y}), target Y: {targetLocation.Y}, verticalVelocity: {verticalVelocity}, missed: {hasMissed}");
            }
            // Обработка обычной торпеды
            else
            {
                // Двигаем торпеду по горизонтали
                nextLocation.X += 5 * DirectionValue;
            }

            // Определяем размер снаряда
            // Определяем размер снаряда
            int width, height;

            switch (TorpedoType)
            {
                case Constants.Ship.TorpedoType.Mine:
                    width = Constants.Ship.MineSize;
                    height = Constants.Ship.MineSize;
                    break;

                case Constants.Ship.TorpedoType.Homing:
                    width = Constants.Ship.HomingProjectileWidth;
                    height = Constants.Ship.HomingProjectileHeight;
                    break;

                default: // Обычный снаряд (или другой тип, если есть)
                    width = Constants.Ship.ProjectileWidth;
                    height = Constants.Ship.ProjectileHeight;
                    break;
            }

            // Проверяем выход за границы экрана
            if (nextLocation.X < -width || nextLocation.X > maxX || nextLocation.Y < -height || nextLocation.Y > maxY)
            {
                Console.WriteLine($"Projectile removed at ({nextLocation.X}, {nextLocation.Y})");
                // Удаляем снаряд
                Form.RemoveProjectile(this);
            }
            else
            {
                // Обновляем позицию
                Location = nextLocation;
            }
        }

        // Отрисовка снаряда
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            Console.WriteLine($"Painting projectile {TorpedoType} at ({Location.X}, {Location.Y})");
            // Отрисовываем текстуру снаряда
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
            base.OnPaint(e);
        }
    }
}