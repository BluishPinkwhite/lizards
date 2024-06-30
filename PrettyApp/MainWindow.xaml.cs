using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrettyApp.drawable;

namespace PrettyApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static Image image;
    public static WriteableBitmap bm { get; private set; }


    public MainWindow()
    {
        InitializeComponent();

        image = new Image();
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(image, EdgeMode.Aliased);

        Content = image;
        Show();

        bm = new WriteableBitmap(
            (int)(ActualWidth / App.Zoom),
            (int)(ActualHeight / App.Zoom),
            48, 48,
            PixelFormats.Bgr32,
            null);

        image.Source = bm;
        image.Stretch = Stretch.UniformToFill;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.VerticalAlignment = VerticalAlignment.Top;

        image.MouseMove += i_MouseMove;
        image.MouseLeftButtonDown += i_MouseLeft;
        image.MouseRightButtonDown += i_MouseRight;

        this.MouseWheel += w_OnMouseWheel;

        App app = (App)Application.Current;
        app.PrepareSimulation();
        app.RunSimulation();
    }

    private void w_OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
    }

    private void i_MouseRight(object sender, MouseButtonEventArgs e)
    {
    }

    private void i_MouseLeft(object sender, MouseButtonEventArgs e)
    {
    }

    private void i_MouseMove(object sender, MouseEventArgs e)
    {
        //DrawPixels((int)Math.Ceiling(e.GetPosition(image).X / Zoom), (int)Math.Ceiling(e.GetPosition(image).Y / Zoom));
    }


    private static void DrawSinglePixel(MouseEventArgs e)
    {
        byte[] color = [0, 200, 50];
        Int32Rect rect = new Int32Rect(
            (int)Math.Ceiling(e.GetPosition(image).X / App.Zoom),
            (int)Math.Ceiling(e.GetPosition(image).Y / App.Zoom),
            1, 1);

        bm.WritePixels(rect, color, 4, 0);
    }


    public static void DrawPixels(IEnumerable<IPixelable> entities)
    {
        try
        {
            bm.Lock();

            unsafe
            {
                foreach (IPixelable entity in entities)
                {
                    List<(int, int, int)> list = entity.GetPixelData();

                    foreach ((int column, int row, int color) in list)
                    {
                        IntPtr pBackBuffer = bm.BackBuffer;

                        pBackBuffer += row * bm.BackBufferStride;
                        pBackBuffer += column * 4;

                        // int color = 0x820000;
                        //
                        // int color_data = 255 << 16; // R
                        // color_data |= 128 << 8;   // G
                        // color_data |= 255 << 0;

                        *((int*) pBackBuffer) = color;
                    
                        bm.AddDirtyRect(new Int32Rect(column, row, 1, 1));
                    }
                }
            }

        }
        finally
        {
            bm.Unlock();
        }
    }

    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
    }
}