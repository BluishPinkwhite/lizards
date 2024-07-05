using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PrettyApp.window;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */

public partial class MainWindow : Window
{
    internal static Image image;
    internal static WriteableBitmap bm { get; private set; }
    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }
    internal static bool WindowSizeChanged = true;

    internal static MainWindow instance;


    public MainWindow()
    {
        instance = this;

        InitializeComponent();

        image = new Image();
        RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
        RenderOptions.SetEdgeMode(image, EdgeMode.Unspecified);
        RenderOptions.SetCachingHint(image, CachingHint.Cache);

        Content = image;
        Show();

        ResizeWindow();

        image.Stretch = Stretch.UniformToFill;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.VerticalAlignment = VerticalAlignment.Top;

        image.MouseMove += i_MouseMove;
        image.MouseLeftButtonDown += i_MouseLeft;
        image.MouseRightButtonDown += i_MouseRight;

        KeyDown += i_KeyDown;

        MouseWheel += w_OnMouseWheel;

        MouseX = bm.PixelWidth / 2;
        MouseY = bm.PixelHeight / 2;

        App app = (App)Application.Current;
        try
        {
            app.PrepareSimulation();
            DrawManager.ClearRenderedScene();
            app.RunSimulation();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }

    private void i_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F)
        {
            if (WindowStyle == WindowStyle.None)
            {
                // set windowed
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
            }
            else
            {
                // set fullscreen
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
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
        MouseX = (int)Math.Ceiling(e.GetPosition(image).X / App.Zoom);
        MouseY = (int)Math.Ceiling(e.GetPosition(image).Y / App.Zoom);
    }


    private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        WindowSizeChanged = true;
    }

    internal static void ResizeWindow()
    {
        WindowSizeChanged = false;

        bm = new WriteableBitmap(
            (int)(instance.ActualWidth / App.Zoom),
            (int)(instance.ActualHeight / App.Zoom),
            48, 48,
            PixelFormats.Bgr32,
            null);

        DrawManager.ClearRenderedScene();

        image.Source = bm;
    }
}