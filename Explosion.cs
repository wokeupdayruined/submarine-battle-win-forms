using System;
using System.Drawing;
using System.Windows.Forms;

namespace sea_battle_C_
{
    // Класс, представляющий анимацию взрыва
    public class Explosion : Control
    {
        // Массив кадров анимации
        private Bitmap[] Frames { get; set; }
        // Текущий кадр анимации
        private int CurrentFrame { get; set; }
        // Ссылка на главную форму
        private Form1 Form { get; set; }
        // Время создания взрыва
        private DateTime SpawnTime { get; set; }
        // Время последней смены кадра
        private DateTime LastFrameChange { get; set; }
        // Длительность отображения одного кадра
        private const double FrameDurationSeconds = Constants.Ship.ExplosionDurationSeconds / 3.0; // Делим общее время на количество кадров
        // Флаг для удаления взрыва
        public bool toRemove { get; set; } = false;

        // Конструктор взрыва
        public Explosion(Form1 form, Point location)
        {
            // Включаем двойную буферизацию для плавной отрисовки
            DoubleBuffered = true;
            // Сохраняем ссылку на форму
            Form = form;
            // Центрируем взрыв относительно переданной позиции
            Location = new Point(
                location.X - Constants.Ship.ExplosionSize / 2,
                location.Y - Constants.Ship.ExplosionSize / 2);
            // Запоминаем время создания
            SpawnTime = DateTime.Now;
            // Устанавливаем время последней смены кадра
            LastFrameChange = SpawnTime;
            // Устанавливаем начальный кадр
            CurrentFrame = 0;

            // Загружаем кадры анимации
            Frames = new Bitmap[Constants.Ship.ExplosionFramePaths.Length];
            for (int i = 0; i < Constants.Ship.ExplosionFramePaths.Length; i++)
            {
                try
                {
                    // Загружаем и масштабируем кадр
                    Frames[i] = new Bitmap(Image.FromFile(Constants.Ship.ExplosionFramePaths[i]), new Size(Constants.Ship.ExplosionSize, Constants.Ship.ExplosionSize));
                    Console.WriteLine($"Loaded explosion frame {i + 1}: {Constants.Ship.ExplosionFramePaths[i]}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading explosion frame {Constants.Ship.ExplosionFramePaths[i]}: {ex.Message}");
                    throw;
                }
            }

            // Устанавливаем размер взрыва
            base.Width = Constants.Ship.ExplosionSize;
            base.Height = Constants.Ship.ExplosionSize;
            // Включаем прозрачный фон
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        // Отрисовка взрыва
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Отрисовываем текущий кадр
            graphics.DrawImageUnscaled(Frames[CurrentFrame], 0, 0);
            base.OnPaint(e);
        }

        // Обновление состояния взрыва
        public new void Update()
        {
            // Проверяем время для смены кадра
            double elapsedSeconds = (DateTime.Now - LastFrameChange).TotalSeconds;
            if (elapsedSeconds >= FrameDurationSeconds && CurrentFrame < Frames.Length - 1)
            {
                // Переключаем на следующий кадр
                CurrentFrame++;
                // Обновляем время смены
                LastFrameChange = DateTime.Now;
                Console.WriteLine($"Explosion switched to frame {CurrentFrame + 1} at ({Location.X}, {Location.Y})");
                // Запрашиваем перерисовку
                Invalidate();
            }

            // Проверяем время жизни взрыва
            if ((DateTime.Now - SpawnTime).TotalSeconds >= Constants.Ship.ExplosionDurationSeconds)
            {
                // Помечаем взрыв для удаления
                toRemove = true;
                Console.WriteLine($"Explosion marked for removal at ({Location.X}, {Location.Y})");
            }
        }

        // Освобождение ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Освобождаем кадры анимации
                foreach (var frame in Frames)
                {
                    frame?.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}