using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace sea_battle_C_
{
    // Перечисление состояний игры
    enum State
    {
        BeforeStart, // Перед началом игры
        InProgress,  // Игра в процессе
        Paused,      // Игра на паузе
        GameOver     // Игра завершена
    }

    // Главная форма игры, управляет логикой и отрисовкой
    public partial class Form1 : Form
    {
        // Настройки игры
        private GameSettings gameSettings = new GameSettings();
        // Первый игрок (корабль)
        BattleShip player1;
        // Второй игрок (корабль)
        BattleShip player2;
        // Статистика побед игроков
        Stats stats = new Stats();
        // Таймер для обновления игрового состояния
        Timer timer = new Timer();
        // Список активных снарядов
        List<Projectile> projectiles = new List<Projectile>();
        // Проигрыватель звука выстрела
        private System.Media.SoundPlayer fireSoundPlayer;
        // Проигрыватель звука взрыва
        private System.Media.SoundPlayer explosionSoundPlayer;
        // Проигрыватель фоновой музыки
        private WMPLib.WindowsMediaPlayer backgroundMusicPlayer;
        // Список активных взрывов
        private List<Explosion> explosions = new List<Explosion>();
        // Список активных бонусов
        private List<PowerUp> powerUps = new List<PowerUp>();
        // Время последнего спавна бонуса
        private DateTime lastPowerUpSpawnTime = DateTime.MinValue;
        // Проигрыватель звука подбора бонуса
        private System.Media.SoundPlayer powerUpSoundPlayer;
        // Генератор случайных чисел
        private Random random = new Random();
        // Текущее состояние игры
        State state = State.BeforeStart;
        // Текст результата игры
        string FinalText = "";
        // Фоновое изображение игрового поля
        Bitmap bitmap = new Bitmap(Image.FromFile(Constants.BackgroundImagePath), Constants.FormWidth, Constants.FormHeight);

        // Конструктор формы
        public Form1()
        {
            // Включаем двойную буферизацию для плавной отрисовки
            DoubleBuffered = true;
            // Открываем консоль для отладки
            AllocConsole();
            // Инициализируем компоненты формы
            InitializeComponent();
            // Включаем предварительную обработку клавиш
            KeyPreview = true;
            // Создаём первого игрока
            player1 = new BattleShip(this, Constants.Ship.PlayerShip.Player1, Constants.FormWidth, Constants.FormHeight, gameSettings);
            // Создаём второго игрока
            player2 = new BattleShip(this, Constants.Ship.PlayerShip.Player2, Constants.FormWidth, Constants.FormHeight, gameSettings);
            // Добавляем игроков на форму
            Controls.Add(player1);
            Controls.Add(player2);
            // Устанавливаем начальные позиции игроков
            player1.Location = new Point(0, Constants.FormHeight / 2);
            player2.Location = new Point(Constants.FormWidth - player1.Width, Constants.FormHeight / 2);
            Console.WriteLine("Player 1 added to Controls");
            Console.WriteLine("Player 2 added to Controls");
            Console.WriteLine($"Initialized Player 1 at ({player1.Location.X}, {player1.Location.Y})");
            Console.WriteLine($"Initialized Player 2 at ({player2.Location.X}, {player2.Location.Y})");
            // Инициализируем список взрывов
            explosions = new List<Explosion>();
            // Инициализируем список бонусов
            powerUps = new List<PowerUp>();
            // Загружаем фоновое изображение
            bitmap = new Bitmap(Image.FromFile(Constants.BackgroundImagePath), Constants.FormWidth, Constants.FormHeight);
            // Удаляем белые пиксели из фона
            Utils.RemoveWhitePixels(ref bitmap);
            // Устанавливаем размер формы
            this.ClientSize = new Size(Constants.FormWidth, Constants.FormHeight);
            // Включаем перерисовку при изменении размера
            this.ResizeRedraw = true;
            // Фиксируем стиль формы
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            // Отключаем кнопку максимального размера
            this.MaximizeBox = false;
            // Включаем обработку клавиш
            this.KeyPreview = true;
            // Подписываемся на события клавиш и отрисовки
            this.KeyDown += new KeyEventHandler(this.Form1_KeyDown);
            this.KeyUp += new KeyEventHandler(this.Form1_KeyUp);
            this.Paint += new PaintEventHandler(this.Form1_Paint);
            // Инициализируем таймер
            InitializeTimer();
            // Запускаем таймер
            timer.Start();
            // Загружаем звук выстрела
            try
            {
                fireSoundPlayer = new System.Media.SoundPlayer(Constants.Ship.FireSoundPath);
                Console.WriteLine($"Loaded fire sound: {Constants.Ship.FireSoundPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading fire sound {Constants.Ship.FireSoundPath}: {ex.Message}");
            }
            // Загружаем звук взрыва
            try
            {
                explosionSoundPlayer = new System.Media.SoundPlayer(Constants.Ship.ExplosionSoundPath);
                Console.WriteLine($"Loaded explosion sound: {Constants.Ship.ExplosionSoundPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading explosion sound {Constants.Ship.ExplosionSoundPath}: {ex.Message}");
            }
            // Загружаем звук подбора бонуса
            try
            {
                powerUpSoundPlayer = new System.Media.SoundPlayer(Constants.Ship.PowerUpSoundPath);
                Console.WriteLine($"Loaded power-up sound: {Constants.Ship.PowerUpSoundPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading power-up sound {Constants.Ship.PowerUpSoundPath}: {ex.Message}");
            }
            // Загружаем и запускаем фоновую музыку
            try
            {
                backgroundMusicPlayer = new WMPLib.WindowsMediaPlayer();
                backgroundMusicPlayer.URL = Constants.Ship.BackgroundMusicPath;
                backgroundMusicPlayer.settings.setMode("loop", true);
                backgroundMusicPlayer.settings.volume = Constants.Ship.BackgroundMusicVolume;
                backgroundMusicPlayer.controls.play();
                Console.WriteLine($"Loaded and started background music: {Constants.Ship.BackgroundMusicPath} at volume {Constants.Ship.BackgroundMusicVolume}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing background music {Constants.Ship.BackgroundMusicPath}: {ex.Message}");
                backgroundMusicPlayer = null;
            }
            // Перерисовываем игроков
            player1.Invalidate();
            player2.Invalidate();
            Console.WriteLine($"Window Size: Width = {this.ClientSize.Width}, Height = {this.ClientSize.Height}");
        }

        // Импорт функции для открытия консоли
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        // Инициализация таймера
        private void InitializeTimer()
        {
            // Устанавливаем интервал таймера (60 FPS)
            timer.Interval = 1000 / 60;
            // Подписываемся на событие тика таймера
            timer.Tick += new EventHandler(StepTimer);
        }

        // Удаление снаряда из игры
        internal void RemoveProjectile(Projectile projectile)
        {
            // Удаляем снаряд из элементов управления
            Controls.Remove(projectile);
            // Помечаем снаряд для удаления
            projectile.toRemove = true;
        }

        // Удаление взрыва из игры
        internal void RemoveExplosion(Explosion explosion)
        {
            // Удаляем взрыв из элементов управления
            Controls.Remove(explosion);
            // Помечаем взрыв для удаления
            explosion.toRemove = true;
        }

        // Добавление взрыва в игру
        internal void AddExplosion(Explosion explosion)
        {
            // Добавляем взрыв в список
            explosions.Add(explosion);
            // Добавляем взрыв на форму
            Controls.Add(explosion);
        }

        // Воспроизведение звука взрыва
        internal void PlayExplosionSound()
        {
            try
            {
                // Проигрываем звук взрыва
                explosionSoundPlayer?.Play();
                Console.WriteLine("Explosion sound played");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing explosion sound: {ex.Message}");
            }
        }

        // Спавн бонуса
        private void SpawnPowerUp()
        {
            // Проверяем интервал спавна бонусов
            if ((DateTime.Now - lastPowerUpSpawnTime).TotalSeconds < gameSettings.PowerUpSpawnIntervalSeconds)
                return;

            // Генерируем случайные координаты для бонуса
            int x = random.Next(50, ClientSize.Width - Constants.Ship.PowerUpSize - 50);
            int y = random.Next(50, ClientSize.Height - Constants.Ship.PowerUpSize - 50);
            // Выбираем случайный тип бонуса
            var type = (PowerUp.PowerUpType)random.Next(0, 3);
            // Создаём бонус
            var powerUp = new PowerUp(this, new Point(x, y), type);
            // Добавляем бонус в список
            powerUps.Add(powerUp);
            // Добавляем бонус на форму
            Controls.Add(powerUp);
            // Обновляем время спавна
            lastPowerUpSpawnTime = DateTime.Now;
            Console.WriteLine($"Spawned {type} power-up at ({x}, {y})");
        }

        // Проверка коллизии с бонусами
        private void CheckPowerUpCollision()
        {
            // Получаем активные бонусы
            var activePowerUps = powerUps.Where(p => !p.toRemove).ToList();
            foreach (var powerUp in activePowerUps)
            {
                // Создаём хитбокс бонуса
                var powerUpRect = new Rectangle(powerUp.Location.X, powerUp.Location.Y, powerUp.Width, powerUp.Height);
                // Проверяем коллизию с первым игроком
                if (player1.ShipBounds.IntersectsWith(powerUpRect))
                {
                    // Применяем эффект бонуса
                    player1.ApplyPowerUp(powerUp.Type);
                    // Помечаем бонус для удаления
                    powerUp.toRemove = true;
                    // Удаляем бонус из элементов управления
                    Controls.Remove(powerUp);
                    // Удаляем бонус из списка
                    powerUps.Remove(powerUp);
                    try
                    {
                        // Проигрываем звук подбора бонуса
                        powerUpSoundPlayer?.Play();
                        Console.WriteLine("Power-up sound played");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error playing power-up sound: {ex.Message}");
                    }
                    Console.WriteLine($"Player 1 collected {powerUp.Type} power-up");
                }
                // Проверяем коллизию со вторым игроком
                else if (player2.ShipBounds.IntersectsWith(powerUpRect))
                {
                    // Применяем эффект бонуса
                    player2.ApplyPowerUp(powerUp.Type);
                    // Помечаем бонус для удаления
                    powerUp.toRemove = true;
                    // Удаляем бонус из элементов управления
                    Controls.Remove(powerUp);
                    // Удаляем бонус из списка
                    powerUps.Remove(powerUp);
                    try
                    {
                        // Проигрываем звук подбора бонуса
                        powerUpSoundPlayer?.Play();
                        Console.WriteLine("Power-up sound played");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error playing power-up sound: {ex.Message}");
                    }
                    Console.WriteLine($"Player 2 collected {powerUp.Type} power-up");
                }
            }
        }

        // Проверка коллизии снаряда с кораблями
        public void CheckCollision(Projectile projectile)
        {
            // Пропускаем мины (их коллизия обрабатывается в Projectile)
            if (projectile.TorpedoType == Constants.Ship.TorpedoType.Mine)
                return;

            // Создаём хитбокс снаряда
            var projectileHitbox = new Rectangle(
                projectile.Location.X,
                projectile.Location.Y,
                Constants.Ship.ProjectileWidth,
                Constants.Ship.ProjectileHeight);

            // Проверяем попадание во первого игрока
            if (player1.ShipBounds.IntersectsWith(projectileHitbox) && projectile.PlayerShip != Constants.Ship.PlayerShip.Player1)
            {
                Console.WriteLine("Player 1 has been shot");
                // Наносим урон
                player1.Hit(projectile);
                // Центрируем взрыв на снаряде
                var explosionLocation = new Point(
                    projectile.Location.X + Constants.Ship.ProjectileWidth / 2,
                    projectile.Location.Y + Constants.Ship.ProjectileHeight / 2);
                // Создаём взрыв
                var explosion = new Explosion(this, explosionLocation);
                AddExplosion(explosion);
                Console.WriteLine("Explosion effect spawned for projectile hit on Player 1");
                // Проигрываем звук взрыва
                PlayExplosionSound();
                // Удаляем снаряд
                RemoveProjectile(projectile);
                // Проверяем, уничтожен ли игрок
                if (player1.Health <= 0)
                {
                    // Увеличиваем счёт побед второго игрока
                    this.stats.player2Wins++;
                    FinalText = "Игрок 1 победил!";
                    // Завершаем игру
                    state = State.GameOver;
                    // Сбрасываем состояние игроков
                    player1.Reinitialize();
                    player2.Reinitialize();
                    Invalidate();
                }
            }
            // Проверяем попадание во второго игрока
            else if (player2.ShipBounds.IntersectsWith(projectileHitbox) && projectile.PlayerShip != Constants.Ship.PlayerShip.Player2)
            {
                Console.WriteLine("Player 2 has been shot");
                // Наносим урон
                player2.Hit(projectile);
                // Центрируем взрыв на снаряде
                var explosionLocation = new Point(
                    projectile.Location.X + Constants.Ship.ProjectileWidth / 2,
                    projectile.Location.Y + Constants.Ship.ProjectileHeight / 2);
                // Создаём взрыв
                var explosion = new Explosion(this, explosionLocation);
                AddExplosion(explosion);
                Console.WriteLine("Explosion effect spawned for projectile hit on Player 2");
                // Проигрываем звук взрыва
                PlayExplosionSound();
                // Удаляем снаряд
                RemoveProjectile(projectile);
                // Проверяем, уничтожен ли игрок
                if (player2.Health <= 0)
                {
                    // Увеличиваем счёт побед первого игрока
                    this.stats.player1Wins++;
                    FinalText = "Игрок 2 победил!";
                    // Завершаем игру
                    state = State.GameOver;
                    Invalidate();
                }
            }
        }

        // Проверка препятствий для движения корабля
        public bool HasObstacle(Rectangle rectangle, BattleShip movingShip)
        {
            // Получаем другой корабль
            var otherShip = movingShip == player1 ? player2 : player1;
            // Проверяем пересечение с другим кораблём
            return otherShip.ShipBounds.IntersectsWith(rectangle);
        }

        // Получение первого игрока
        internal BattleShip GetPlayer1() => player1;
        // Получение второго игрока
        internal BattleShip GetPlayer2() => player2;

        // Обработчик загрузки формы
        private void Form1_Load(object sender, EventArgs e) { }

        // Обработчик тика таймера
        private void StepTimer(object sender, EventArgs e)
        {
            // Выбираем действие в зависимости от состояния игры
            switch (state)
            {
                case State.BeforeStart:
                    this.StepTimerBeforeStart(sender, e);
                    break;
                case State.InProgress:
                    this.StepTimerInProgress(sender, e);
                    break;
                case State.GameOver:
                    this.StepTimerGameOver(sender, e);
                    break;
            }
        }

        // Обновление состояния перед началом игры
        private void StepTimerBeforeStart(object sender, EventArgs e) { }

        // Обновление состояния во время игры
        private void StepTimerInProgress(object sender, EventArgs e)
        {
            // Двигаем корабли
            player1.MoveShip();
            player2.MoveShip();

            // Обновляем снаряды
            var activeProjectiles = projectiles.Where(x => !x.toRemove).ToList();
            foreach (var projectile in activeProjectiles)
            {
                // Двигаем снаряд
                projectile.MoveProjectile();
                // Проверяем коллизии (кроме мин)
                if (projectile.TorpedoType != Constants.Ship.TorpedoType.Mine)
                    CheckCollision(projectile);
            }

            // Обновляем взрывы
            var activeExplosions = explosions.Where(x => !x.toRemove).ToList();
            foreach (var explosion in activeExplosions)
            {
                // Обновляем анимацию взрыва
                explosion.Update();
            }

            // Спавним бонусы
            SpawnPowerUp();
            // Проверяем коллизии с бонусами
            CheckPowerUpCollision();

            // Удаляем помеченные снаряды
            var toRemoveProjectiles = projectiles.Where(x => x.toRemove).ToList();
            foreach (var projectile in toRemoveProjectiles)
            {
                projectiles.Remove(projectile);
                Controls.Remove(projectile);
                projectile.Dispose();
            }

            // Удаляем помеченные взрывы
            var toRemoveExplosions = explosions.Where(x => x.toRemove).ToList();
            foreach (var explosion in toRemoveExplosions)
            {
                explosions.Remove(explosion);
                Controls.Remove(explosion);
                explosion.Dispose();
                Console.WriteLine("Explosion removed");
            }

            // Удаляем помеченные бонусы
            var toRemovePowerUps = powerUps.Where(x => x.toRemove).ToList();
            foreach (var powerUp in toRemovePowerUps)
            {
                powerUps.Remove(powerUp);
                Controls.Remove(powerUp);
                powerUp.Dispose();
            }

            // Проверяем здоровье игроков
            if (player1.Health <= 0)
            {
                // Второй игрок победил
                this.stats.player2Wins++;
                FinalText = "Player 2 won!";
                state = State.GameOver;
                player1.Reinitialize();
                player2.Reinitialize();
                Invalidate();
            }
            else if (player2.Health <= 0)
            {
                // Первый игрок победил
                this.stats.player1Wins++;
                FinalText = "Player 1 won!";
                state = State.GameOver;
                Invalidate();
            }

            // Перерисовываем форму
            Invalidate();
        }

        // Обновление состояния после окончания игры
        private void StepTimerGameOver(object sender, EventArgs e) { }

        // Создание снаряда
        public void CreateProjectile(Constants.Ship.PlayerShip playerShip, Constants.Ship.TorpedoType torpedoType)
        {
            // Определяем стреляющий корабль
            BattleShip trigger;
            if (playerShip == Constants.Ship.PlayerShip.Player1) trigger = player1; else trigger = player2;

            // Проверяем возможность выстрела
            if (!trigger.CanFire(torpedoType))
                return;

            Console.WriteLine($"Creating projectile of type: {torpedoType}");
            // Создаём снаряд
            var projectile = new Projectile(this, playerShip, torpedoType, this.ClientSize.Width, this.ClientSize.Height, gameSettings);

            // Вычисляем центр корабля
            int shipCenterX = trigger.Location.X + trigger.Width / 2;
            int shipCenterY = trigger.Location.Y + trigger.Height / 2;

            // Определяем размер снаряда
            int projectileWidth = torpedoType == Constants.Ship.TorpedoType.Mine ? Constants.Ship.MineSize : Constants.Ship.ProjectileWidth;
            int projectileHeight = torpedoType == Constants.Ship.TorpedoType.Mine ? Constants.Ship.MineSize : Constants.Ship.ProjectileHeight;

            // Позиционируем снаряд перед кораблём
            Point location = new Point(
                shipCenterX + (trigger.Width / 2 + projectileWidth / 2 + 20) * projectile.DirectionValue - projectileWidth / 2,
                shipCenterY - projectileHeight / 2
            );

            // Устанавливаем позицию снаряда
            projectile.Location = location;
            // Устанавливаем начальную позицию для мины
            if (torpedoType == Constants.Ship.TorpedoType.Mine)
            {
                typeof(Projectile).GetField("initialLocation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(projectile, location);
            }
            // Добавляем снаряд в список
            projectiles.Add(projectile);
            // Добавляем снаряд на форму
            Controls.Add(projectile);
            // Выполняем выстрел
            trigger.Fire(torpedoType);

            // Проигрываем звук выстрела
            try
            {
                fireSoundPlayer?.Play();
                Console.WriteLine("Fire sound played");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing fire sound: {ex.Message}");
            }
        }

        // Обработчик нажатия клавиш
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine("key: " + e.KeyCode);
            // Переключение паузы
            if (e.KeyCode == Keys.F4 && (state == State.InProgress || state == State.Paused))
            {
                if (state == State.InProgress)
                {
                    // Ставим игру на паузу
                    state = State.Paused;
                    timer.Stop();
                    try
                    {
                        // Приостанавливаем музыку
                        backgroundMusicPlayer?.controls.pause();
                        Console.WriteLine("Game paused, music paused");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error pausing music: {ex.Message}");
                    }
                    Invalidate();
                }
                else
                {
                    // Возобновляем игру
                    state = State.InProgress;
                    timer.Start();
                    try
                    {
                        // Возобновляем музыку
                        backgroundMusicPlayer?.controls.play();
                        Console.WriteLine("Game resumed, music resumed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error resuming music: {ex.Message}");
                    }
                    Invalidate();
                }
            }
            // Обработка клавиш на паузе
            else if (state == State.Paused)
            {
                // Открытие настроек
                if (e.KeyCode == Keys.F2)
                {
                    using (var settingsForm = new SettingsForm(gameSettings))
                    {
                        if (settingsForm.ShowDialog() == DialogResult.OK)
                        {
                            // Удаляем старые корабли
                            if (player1 != null)
                            {
                                Controls.Remove(player1);
                                player1.Dispose();
                            }
                            if (player2 != null)
                            {
                                Controls.Remove(player2);
                                player2.Dispose();
                            }

                            // Создаём новые корабли с новыми настройками
                            player1 = new BattleShip(this, Constants.Ship.PlayerShip.Player1, Constants.FormWidth, Constants.FormHeight, gameSettings);
                            player2 = new BattleShip(this, Constants.Ship.PlayerShip.Player2, Constants.FormWidth, Constants.FormHeight, gameSettings);
                            Controls.Add(player1);
                            Controls.Add(player2);
                            player1.Location = new Point(0, Constants.FormHeight / 2);
                            player2.Location = new Point(Constants.FormWidth - player1.Width, Constants.FormHeight / 2);
                            Console.WriteLine("Players reinitialized with new settings");

                            // Перезапускаем игру
                            state = State.InProgress;
                            Reset();
                            timer.Start();
                            try
                            {
                                // Возобновляем музыку
                                backgroundMusicPlayer?.controls.play();
                                Console.WriteLine("Game restarted, music resumed");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error resuming music: {ex.Message}");
                            }
                            Invalidate();
                        }
                    }
                }
                // Перезапуск игры
                else if (e.KeyCode == Keys.Enter)
                {
                    state = State.InProgress;
                    Reset();
                    timer.Start();
                    try
                    {
                        // Возобновляем музыку
                        backgroundMusicPlayer?.controls.play();
                        Console.WriteLine("Game restarted, music resumed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error resuming music: {ex.Message}");
                    }
                    Invalidate();
                }
            }
            else
            {
                // Обработка клавиш в других состояниях
                switch (state)
                {
                    case State.BeforeStart:
                        Form1_KeyDownBeforeStart(sender, e);
                        break;
                    case State.InProgress:
                        Form1_KeyDownInProgress(sender, e);
                        break;
                    case State.GameOver:
                        Form1_KeyDownGameOver(sender, e);
                        break;
                }
            }
        }

        // Обработчик клавиш перед началом игры
        private void Form1_KeyDownBeforeStart(object sender, KeyEventArgs e)
        {
            // Открытие настроек
            if (e.KeyCode == Keys.F2)
            {
                using (var settingsForm = new SettingsForm(gameSettings))
                {
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        // Удаляем старые корабли
                        if (player1 != null)
                        {
                            Controls.Remove(player1);
                            player1.Dispose();
                        }
                        if (player2 != null)
                        {
                            Controls.Remove(player2);
                            player2.Dispose();
                        }

                        // Создаём новые корабли с новыми настройками
                        player1 = new BattleShip(this, Constants.Ship.PlayerShip.Player1, Constants.FormWidth, Constants.FormHeight, gameSettings);
                        player2 = new BattleShip(this, Constants.Ship.PlayerShip.Player2, Constants.FormWidth, Constants.FormHeight, gameSettings);
                        Controls.Add(player1);
                        Controls.Add(player2);
                        player1.Location = new Point(0, Constants.FormHeight / 2);
                        player2.Location = new Point(Constants.FormWidth - player1.Width, Constants.FormHeight / 2);
                        Console.WriteLine("Players reinitialized with new settings");
                    }
                }
                Invalidate();
            }
            // Запуск игры
            else if (e.KeyCode == Keys.Enter)
            {
                state = State.InProgress;
                Reset();
                try
                {
                    // Запускаем музыку
                    backgroundMusicPlayer?.controls.play();
                    Console.WriteLine("Background music started");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting music: {ex.Message}");
                }
                Invalidate();
            }
        }

        // Сброс игрового состояния
        private void Reset()
        {
            // Очищаем текст результата
            FinalText = "";
            // Сбрасываем позиции игроков
            player1.Location = new Point(0, this.ClientSize.Height / 2);
            player2.Location = new Point(this.ClientSize.Width - player1.Width, this.ClientSize.Height / 2);
            // Удаляем все снаряды
            foreach (var projectile in projectiles)
            {
                Controls.Remove(projectile);
            }
            projectiles.Clear();
            // Удаляем все бонусы
            foreach (var powerUp in powerUps)
            {
                Controls.Remove(powerUp);
            }
            powerUps.Clear();
            // Сбрасываем время спавна бонусов
            lastPowerUpSpawnTime = DateTime.MinValue;
            Invalidate();
        }

        // Обработчик клавиш во время игры
        private void Form1_KeyDownInProgress(object sender, KeyEventArgs e)
        {
            // Обработка управления для первого игрока
            switch (e.KeyCode)
            {
                case Constants.Player1Controls.MoveRight:
                    player1.IsMovingRight = true;
                    break;
                case Constants.Player1Controls.MoveDown:
                    player1.IsMovingDown = true;
                    break;
                case Constants.Player1Controls.MoveLeft:
                    player1.IsMovingLeft = true;
                    break;
                case Constants.Player1Controls.MoveUp:
                    player1.IsMovingUp = true;
                    break;
                case Constants.Player1Controls.Fire:
                    CreateProjectile(Constants.Ship.PlayerShip.Player1, Constants.Ship.TorpedoType.Ordinary);
                    break;
                case Constants.Player1Controls.FireHoming:
                    CreateProjectile(Constants.Ship.PlayerShip.Player1, Constants.Ship.TorpedoType.Homing);
                    break;
                case Constants.Player1Controls.FireMine:
                    CreateProjectile(Constants.Ship.PlayerShip.Player1, Constants.Ship.TorpedoType.Mine);
                    break;
            }
            // Обработка управления для второго игрока
            switch (e.KeyCode)
            {
                case Constants.Player2Controls.MoveRight:
                    player2.IsMovingRight = true;
                    break;
                case Constants.Player2Controls.MoveDown:
                    player2.IsMovingDown = true;
                    break;
                case Constants.Player2Controls.MoveLeft:
                    player2.IsMovingLeft = true;
                    break;
                case Constants.Player2Controls.MoveUp:
                    player2.IsMovingUp = true;
                    break;
                case Constants.Player2Controls.Fire:
                    CreateProjectile(Constants.Ship.PlayerShip.Player2, Constants.Ship.TorpedoType.Ordinary);
                    break;
                case Constants.Player2Controls.FireHoming:
                    CreateProjectile(Constants.Ship.PlayerShip.Player2, Constants.Ship.TorpedoType.Homing);
                    break;
                case Constants.Player2Controls.FireMine:
                    CreateProjectile(Constants.Ship.PlayerShip.Player2, Constants.Ship.TorpedoType.Mine);
                    break;
            }
        }

        // Обработчик клавиш после окончания игры
        private void Form1_KeyDownGameOver(object sender, KeyEventArgs e)
        {
            // Возвращаемся в начальное состояние
            state = State.BeforeStart;
            // Очищаем текст результата
            FinalText = "";
            // Сбрасываем состояние игроков
            player1.Reinitialize();
            player2.Reinitialize();
            // Сбрасываем игру
            Reset();
            try
            {
                // Останавливаем музыку
                backgroundMusicPlayer?.controls.stop();
                Console.WriteLine("Background music stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping music: {ex.Message}");
            }
            Invalidate();
        }

        // Обработчик отпускания клавиш
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // Отключаем движение для первого игрока
            switch (e.KeyCode)
            {
                case Constants.Player1Controls.MoveRight:
                    player1.IsMovingRight = false;
                    break;
                case Constants.Player1Controls.MoveDown:
                    player1.IsMovingDown = false;
                    break;
                case Constants.Player1Controls.MoveLeft:
                    player1.IsMovingLeft = false;
                    break;
                case Constants.Player1Controls.MoveUp:
                    player1.IsMovingUp = false;
                    break;
            }
            // Отключаем движение для второго игрока
            switch (e.KeyCode)
            {
                case Constants.Player2Controls.MoveRight:
                    player2.IsMovingRight = false;
                    break;
                case Constants.Player2Controls.MoveDown:
                    player2.IsMovingDown = false;
                    break;
                case Constants.Player2Controls.MoveLeft:
                    player2.IsMovingLeft = false;
                    break;
                case Constants.Player2Controls.MoveUp:
                    player2.IsMovingUp = false;
                    break;
            }
        }

        // Обработчик отрисовки формы
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Выбираем метод отрисовки в зависимости от состояния
            switch (state)
            {
                case State.BeforeStart:
                    Form1_PaintBeforeStart(sender, e);
                    break;
                case State.InProgress:
                    Form1_PaintInProgress(sender, e);
                    break;
                case State.Paused:
                    Form1_PaintPaused(sender, e);
                    break;
                case State.GameOver:
                    Form1_PaintGameOver(sender, e);
                    break;
            }
        }

        // Отрисовка перед началом игры
        private void Form1_PaintBeforeStart(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Отрисовываем фон
            graphics.DrawImageUnscaled(bitmap, 0, 0);
            // Отрисовываем подсказки
            graphics.DrawString("Нажмите Enter клавишу чтобы начать", new Font("Arial", 20), Brushes.Black, new Point(this.ClientSize.Width / 2 - 250, 0));
            graphics.DrawString("Нажмите F2 для настроек", new Font("Arial", 16), Brushes.Black, new Point(this.ClientSize.Width / 2 - 150, 40));
        }

        // Отрисовка во время игры
        private void Form1_PaintInProgress(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Отрисовываем фон
            graphics.DrawImageUnscaled(bitmap, 0, 0);

            // Отрисовываем полоски здоровья
            DrawHealthBar(graphics, player1, new Point(player1.Location.X, player1.Location.Y - 20));
            DrawHealthBar(graphics, player2, new Point(player2.Location.X, player2.Location.Y - 20));

            // Отрисовываем отладочные эллипсы для мин
            foreach (var projectile in projectiles)
            {
                if (projectile.TorpedoType == Constants.Ship.TorpedoType.Mine && !projectile.toRemove)
                {
                    var explosionRect = new Rectangle(
                        projectile.Location.X - (Constants.Ship.MineExplosionRadius - Constants.Ship.MineSize) / 2,
                        projectile.Location.Y - (Constants.Ship.MineExplosionRadius - Constants.Ship.MineSize) / 2,
                        Constants.Ship.MineExplosionRadius,
                        Constants.Ship.MineExplosionRadius);
                    graphics.DrawEllipse(Pens.Red, explosionRect);
                }
            }
        }

        // Отрисовка на паузе
        private void Form1_PaintPaused(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Отрисовываем фон
            graphics.DrawImageUnscaled(bitmap, 0, 0);

            // Отрисовываем полоски здоровья
            DrawHealthBar(graphics, player1, new Point(player1.Location.X, player1.Location.Y - 20));
            DrawHealthBar(graphics, player2, new Point(player2.Location.X, player2.Location.Y - 20));

            // Отрисовываем сообщения паузы
            string pauseText = "Пауза";
            string resumeText = "F4: Продолжить";
            string settingsText = "F2: Настройки (перезапуск)";
            string restartText = "Enter: Перезапуск";
            using (Font largeFont = new Font("Arial", 20))
            {
                SizeF pauseSize = graphics.MeasureString(pauseText, largeFont);
                graphics.DrawString(pauseText, largeFont, Brushes.Black,
                    (ClientSize.Width - pauseSize.Width) / 2,
                    (ClientSize.Height - pauseSize.Height) / 2 - 60);
            }
            using (Font smallFont = new Font("Arial", 16))
            {
                SizeF resumeSize = graphics.MeasureString(resumeText, smallFont);
                SizeF settingsSize = graphics.MeasureString(settingsText, smallFont);
                SizeF restartSize = graphics.MeasureString(restartText, smallFont);
                graphics.DrawString(resumeText, smallFont, Brushes.Black,
                    (ClientSize.Width - resumeSize.Width) / 2,
                    (ClientSize.Height - resumeSize.Height) / 2 - 20);
                graphics.DrawString(settingsText, smallFont, Brushes.Black,
                    (ClientSize.Width - settingsSize.Width) / 2,
                    (ClientSize.Height - settingsSize.Height) / 2 + 20);
                graphics.DrawString(restartText, smallFont, Brushes.Black,
                    (ClientSize.Width - restartSize.Width) / 2,
                    (ClientSize.Height - restartSize.Height) / 60);
            }
        }

        // Отрисовка полоски здоровья
        private void DrawHealthBar(Graphics graphics, BattleShip ship, Point position)
        {
            const int barWidth = 80; // Ширина полоски (равна ширине корабля)
            const int barHeight = 10; // Высота полоски
            // Вычисляем процент здоровья
            float healthPercentage = (float)ship.Health / gameSettings.StartHealth;
            // Вычисляем заполненную часть полоски
            int filledWidth = (int)(barWidth * healthPercentage);

            // Отрисовываем фон полоски (красный для утраченного здоровья)
            graphics.FillRectangle(Brushes.Red, position.X, position.Y, barWidth, barHeight);

            // Отрисовываем заполненную часть (градиент от зелёного к красному)
            if (filledWidth > 0)
            {
                Color healthColor = Color.FromArgb(
                    (int)(255 * (1 - healthPercentage)), // Красный увеличивается при уменьшении здоровья
                    (int)(255 * healthPercentage),       // Зелёный уменьшается при уменьшении здоровья
                    0);
                using (Brush healthBrush = new SolidBrush(healthColor))
                {
                    graphics.FillRectangle(healthBrush, position.X, position.Y, filledWidth, barHeight);
                }
            }

            // Отрисовываем рамку полоски
            graphics.DrawRectangle(Pens.Black, position.X, position.Y, barWidth, barHeight);

            // Отрисовываем текст здоровья
            string healthText = $"{ship.Health}/{gameSettings.StartHealth}";
            using (Font font = new Font("Arial", 8))
            {
                SizeF textSize = graphics.MeasureString(healthText, font);
                graphics.DrawString(healthText, font, Brushes.Black,
                    position.X + (barWidth - textSize.Width) / 2,
                    position.Y + (barHeight - textSize.Height) / 2);
            }
        }

        // Отрисовка после окончания игры
        private void Form1_PaintGameOver(object sender, PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Отрисовываем фон
            graphics.DrawImageUnscaled(bitmap, 0, 0);
            // Отрисовываем статистику игроков
            var playersStat = $"{this.stats.player1Wins}:{this.stats.player2Wins}";
            graphics.DrawString(FinalText, new Font("Arial", 20), Brushes.Black, new Point(this.ClientSize.Width / 2 - 100, 0));
            graphics.DrawString(playersStat, new Font("Arial", 20), Brushes.Black, new Point(this.ClientSize.Width / 2 - 35, 23));

            // Очищаем бонусы
            foreach (var powerUp in powerUps)
            {
                Controls.Remove(powerUp);
            }
            powerUps.Clear();
        }

        // Переопределение обработки клавиш
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            // Открытие справки
            if (e.KeyCode == Keys.F1)
            {
                using (var helpForm = new HelpForm())
                {
                    helpForm.ShowDialog();
                }
            }
        }
    }
}