using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace sea_battle_C_
{
    // Класс PowerUp представляет бонус, который может подобрать игрок
    public class PowerUp : Control
    {
        // Перечисление типов бонусов
        public enum PowerUpType
        {
            SpeedBoost,     // Ускорение движения
            FireRateBoost,  // Ускорение перезарядки оружия
            HealthBoost     // Восстановление здоровья
        }

        // Изображение бонуса
        private Bitmap Bitmap { get; set; }
        // Тип текущего бонуса
        public PowerUpType Type { get; private set; }
        // Ссылка на главную форму игры
        private Form1 Form { get; set; }
        // Флаг, указывающий, нужно ли удалить бонус
        public bool toRemove { get; set; } = false;

        // Конструктор бонуса
        public PowerUp(Form1 form, Point location, PowerUpType type)
        {
            // Включаем двойную буферизацию для плавного отображения
            DoubleBuffered = true;
            Form = form;
            Type = type;
            // Получаем путь к изображению бонуса на основе его типа
            string imagePath = Constants.Ship.PowerUpImagePaths[(int)type];
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
            // Проверяем, существует ли файл изображения
            if (!File.Exists(fullPath))
            {
                // Если изображение не найдено, используем запасное изображение мины
                imagePath = Constants.Ship.MineProjectilePath;
                fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                Console.WriteLine($"Изображение бонуса не найдено по пути {fullPath}, используется запасное: {imagePath}");
            }
            else
            {
                Console.WriteLine($"Загрузка изображения бонуса: {fullPath}");
            }
            try
            {
                // Загружаем и масштабируем изображение бонуса
                Bitmap = new Bitmap(Image.FromFile(fullPath), new Size(Constants.Ship.PowerUpSize, Constants.Ship.PowerUpSize));
            }
            catch (Exception ex)
            {
                // Обрабатываем ошибку загрузки изображения
                Console.WriteLine($"Ошибка загрузки изображения бонуса {fullPath}: {ex.Message}");
                throw;
            }
            // Устанавливаем размеры бонуса
            Width = Constants.Ship.PowerUpSize;
            Height = Constants.Ship.PowerUpSize;
            // Устанавливаем начальную позицию бонуса
            Location = location;
            // Устанавливаем прозрачный фон
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        // Метод отрисовки бонуса
        protected override void OnPaint(PaintEventArgs e)
        {
            var graphics = e.Graphics;
            // Рисуем изображение бонуса
            graphics.DrawImageUnscaled(Bitmap, 0, 0);
            base.OnPaint(e);
        }
    }
}