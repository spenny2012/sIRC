using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace spennyIRC.Helpers;

public static class IrcWindowHelper
{
    public static void SetTiledBackground(this RichTextBox richTextBox, string resource)
    {
        Uri resourceLocater = new(resource, UriKind.Relative);
        StreamResourceInfo resourceStream = Application.GetResourceStream(resourceLocater);
        if (resourceStream != null)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.StreamSource = resourceStream.Stream;
            bitmap.EndInit();

            ImageBrush backgroundBrush = new(bitmap)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 0.10, 0.10),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };

            richTextBox.Background = backgroundBrush;
        }
    }
    public static void SetBackground(this RichTextBox richTextBox, string resource)
    {
        Uri resourceLocater = new(resource, UriKind.Relative);
        StreamResourceInfo resourceStream = Application.GetResourceStream(resourceLocater);
        if (resourceStream != null)
        {
            BitmapImage bitmap = new();
            bitmap.BeginInit();
            bitmap.StreamSource = resourceStream.Stream;
            bitmap.EndInit();

            ImageBrush backgroundBrush = new(bitmap);

            richTextBox.Background = backgroundBrush;
        }
    }
}
