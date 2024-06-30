using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static Image image;
    public static WriteableBitmap bm { get; private set; }
    public static int MouseX { get; private set; }
    public static int MouseY { get; private set; }


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
        image.Stretch = Stretch.None;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.VerticalAlignment = VerticalAlignment.Top;

        image.MouseMove += i_MouseMove;
        image.MouseLeftButtonDown += i_MouseLeft;
        image.MouseRightButtonDown += i_MouseRight;

        this.MouseWheel += w_OnMouseWheel;

        App app = (App)Application.Current;
        app.PrepareSimulation();
        ClearRenderedScene();
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
        MouseX = (int)Math.Ceiling(e.GetPosition(image).X / App.Zoom);
        MouseY = (int)Math.Ceiling(e.GetPosition(image).Y / App.Zoom);
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


    public static void DrawPixels(IEnumerable<Entity> entities)
    {
        try
        {
            bm.Lock();

            unsafe
            {
                foreach (Entity entity in entities)
                {
                    List<Pixel> list = entity.GetPixelData();
                    
                    BoundingBox bounds = entity.GetBoundingBox();
                    bounds.ClampToScreen(bm.PixelWidth, bm.PixelHeight);

                    if (entity.HasJustUpdated)
                    {
                        entity.HasJustUpdated = false;
                        BoundingBox lastBounds = entity.GetLastBoundingBox();
                        lastBounds.ClampToScreen(bm.PixelWidth, bm.PixelHeight);
                        
                        ResetBackground(lastBounds);
                        bm.AddDirtyRect(new Int32Rect(lastBounds.X, lastBounds.Y, lastBounds.Width() + 1, lastBounds.Height() + 1));
                    }

                    ResetBackground(bounds);

                    foreach (Pixel p in list)
                    {
                        if (p.X < 0 || p.Y < 0 || p.X >= bm.PixelWidth || p.Y > bm.PixelHeight)
                        {
                            Console.Out.WriteLine($"Pixel outside image: ({p.X},{p.Y}), {p.Color:X}, skipping...");
                            continue;
                        }

                        IntPtr pBackBuffer = bm.BackBuffer;

                        pBackBuffer += p.Y * bm.BackBufferStride;
                        pBackBuffer += p.X * 4;

                        *((int*)pBackBuffer) = p.Color;
                    }

                    bm.AddDirtyRect(new Int32Rect(bounds.X, bounds.Y, bounds.Width() + 1, bounds.Height() + 1));
                }
            }
        }
        finally
        {
            bm.Unlock();
        }
    }


    // CALL ONLY WHEN WritableBitmap IS LOCKED
    private static void ResetBackground(BoundingBox bounds)
    {
        unsafe
        {
            // TODO: block by block cache processing?
            
            for (int y = bounds.Y; y <= bounds.Ey; y++)
            {
                for (int x = bounds.X; x <= bounds.Ex; x++)
                {
                    IntPtr pBackBuffer = bm.BackBuffer;

                    pBackBuffer += y * bm.BackBufferStride;
                    pBackBuffer += x * 4;

                    *((int*)pBackBuffer) = (int)App.Tiles.Air;
                }
            }
        }
    }

    private static void ClearRenderedScene()
    {
        try
        {
            bm.Lock();

            ResetBackground(new BoundingBox(0, 0, bm.PixelWidth - 1, bm.PixelHeight - 1));

            bm.AddDirtyRect(new Int32Rect(0, 0, bm.PixelWidth, bm.PixelHeight));
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