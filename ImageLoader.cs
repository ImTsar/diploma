using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace GW_1
{
    public class ImageLoader
    {
        string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

        public Image PlusIcon { get; private set; }
        public Image LeftArrowIcon { get; private set; }
        public Image RightArrowIcon { get; private set; }
        public Image TopArrowIcon { get; private set; }
        public Image BottomArrowIcon { get; private set; }

        public ImageLoader()
        {
            PlusIcon = LoadImage("add_icon.png");
            BottomArrowIcon = LoadImage("bottom-arrow.png");
            TopArrowIcon = LoadImage("top-arrow.png");
            LeftArrowIcon = LoadImage("left-arrow.png");
            RightArrowIcon = LoadImage("right-arrow.png");
        }

        private Image LoadImage(string fileName)
        {
            var fullPath = Path.Combine(currentDirectory, $"..\\..\\..\\Images\\{fileName}");
            if (File.Exists(fullPath))
            {
                return Image.FromFile(fullPath);
            }
            throw new ArgumentException($"File {fileName} not found in {fullPath}");
        }

        public Image GetArrowIcon(string direction)
        {
            return direction switch
            {
                "left" => LeftArrowIcon,
                "right" => RightArrowIcon,
                "top" => TopArrowIcon,
                "bottom" => BottomArrowIcon,
                _ => throw new ArgumentException("Invalid direction")
            };
        }

        public Image GetOppositeArrowIcon(Image arrowIcon)
        {
            if (arrowIcon == LeftArrowIcon) return RightArrowIcon;
            if (arrowIcon == RightArrowIcon) return LeftArrowIcon;
            if (arrowIcon == TopArrowIcon) return BottomArrowIcon;
            if (arrowIcon == BottomArrowIcon) return TopArrowIcon;
            throw new ArgumentException("Invalid arrow icon");
        }

        public ProcessorElementUI.Direction GetDirectionFromImage(Image image)
        {
            if (image == LeftArrowIcon) return ProcessorElementUI.Direction.Left;
            if (image == RightArrowIcon) return ProcessorElementUI.Direction.Right;
            if (image == TopArrowIcon) return ProcessorElementUI.Direction.Up;
            if (image == BottomArrowIcon) return ProcessorElementUI.Direction.Down;
            return ProcessorElementUI.Direction.None;
        }
    }
}