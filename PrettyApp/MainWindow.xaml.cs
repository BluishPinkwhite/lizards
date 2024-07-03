using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp;
/*
 * @author Tammie Hladilů, @BluishPinkwhite on GitHub
 */
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
        image.Stretch = Stretch.UniformToFill;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.VerticalAlignment = VerticalAlignment.Top;

        image.MouseMove += i_MouseMove;
        image.MouseLeftButtonDown += i_MouseLeft;
        image.MouseRightButtonDown += i_MouseRight;

        this.MouseWheel += w_OnMouseWheel;

        MouseX = bm.PixelWidth / 2;
        MouseY = bm.PixelHeight / 2;

        App app = (App)Application.Current;
        try
        {
            app.PrepareSimulation();
            ClearRenderedScene();
            app.RunSimulation();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
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


    private static void DrawSinglePixel(MouseEventArgs e)
    {
        byte[] color = [0, 200, 50];
        Int32Rect rect = new Int32Rect(
            (int)Math.Ceiling(e.GetPosition(image).X / App.Zoom),
            (int)Math.Ceiling(e.GetPosition(image).Y / App.Zoom),
            1, 1);

        bm.WritePixels(rect, color, 4, 0);
    }


    public static void DrawPixels(List<Entity> entities)
    {
        try
        {
            bm.Lock();

            unsafe
            {
                // compress bounding boxes:
                // first dimension: rows (of bm)
                // second dimension: start column index + end column index of bounding boxes in that row 
                Dictionary<int, List<Interval>> rowsBoundingBoxes = new Dictionary<int, List<Interval>>();

                // add all in-bounding-box rows to dictionary
                foreach (Entity entity in entities)
                {
                    BoundingBox bounds = entity.GetBoundingBox();
                    bounds.ClampToScreen(bm.PixelWidth - 1, bm.PixelHeight - 1);
                    bm.AddDirtyRect(new Int32Rect(bounds.X, bounds.Y, bounds.Width() + 1, bounds.Height() + 1));

                    for (int row = bounds.Y; row <= bounds.Ey; row++)
                    {
                        // init row if not initialized
                        if (!rowsBoundingBoxes.ContainsKey(row))
                            rowsBoundingBoxes[row] = new List<Interval>();
                        
                        // add column interval of this bounding box to this row
                        rowsBoundingBoxes[row].Add(new Interval(bounds.X, bounds.Ex));
                    }

                    if (entity.HasJustUpdated)
                    {
                        entity.HasJustUpdated = false;
                        BoundingBox lastBounds = entity.GetLastBoundingBox();
                        lastBounds.ClampToScreen(bm.PixelWidth - 1, bm.PixelHeight - 1);
                        bm.AddDirtyRect(new Int32Rect(lastBounds.X, lastBounds.Y, lastBounds.Width() + 1,
                            lastBounds.Height() + 1));
                        
                        for (int row = lastBounds.Y; row <= lastBounds.Ey; row++)
                        {
                            if (!rowsBoundingBoxes.ContainsKey(row))
                                rowsBoundingBoxes[row] = new List<Interval>();
                            
                            rowsBoundingBoxes[row].Add(new Interval(lastBounds.X, lastBounds.Ex));
                        }
                    }
                }
                
                // merge all overlapping intervals in rows
                foreach (KeyValuePair<int,List<Interval>> keyValuePair in rowsBoundingBoxes)
                {
                    List<Interval> rowIntervals = keyValuePair.Value;
                    Interval[] intervals = rowIntervals.OrderBy(interval => interval.Start).ToArray();

                    int index = 0; // output array index
                    for (int i = 1; i < intervals.Length; i++)
                    {
                        if (intervals[index].End >= intervals[i].Start) 
                        {
                            // merge intervals
                            intervals[index].End = Math.Max(intervals[index].End, intervals[i].End);
                        }
                        else
                        {
                            // move to next for merging into it
                            index++;
                            intervals[index] = intervals[i];
                        }
                    }

                    // result is [0..index-1]
                    rowsBoundingBoxes[keyValuePair.Key] = new List<Interval>(intervals.Take(index + 1));
                }



                // use merged bounding boxes to reset background
                foreach (KeyValuePair<int, List<Interval>> keyValuePair in rowsBoundingBoxes)
                {
                    int row = keyValuePair.Key;
                    List<Interval> columns = keyValuePair.Value;

                    foreach (Interval columnInterval in columns)
                    {
                        ResetRow(row, columnInterval);
                    }
                }


                // draw entities over reset background
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    Entity entity = entities[i];
                    List<Pixel> list = entity.GetPixelData();

                    foreach (Pixel p in list)
                    {
                        int X = p.X;
                        int Y = p.Y;

                        if (X < 0 || Y < 0 || X >= bm.PixelWidth || Y >= bm.PixelHeight)
                        {
                            Console.Out.WriteLine($"Pixel outside image: ({X},{Y}), {p.Color:X}, skipping...");
                            continue;
                        }

                        IntPtr pBackBuffer = bm.BackBuffer;

                        pBackBuffer += Y * bm.BackBufferStride;
                        pBackBuffer += X * 4;

                        *((int*)pBackBuffer) = p.Color;
                    }
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
    
    
    // CALL ONLY WHEN WritableBitmap IS LOCKED
    private static void ResetRow(int row, Interval columnInterval)
    {
        unsafe
        {
            // TODO: block by block cache processing?

            for (int x = columnInterval.Start; x <= columnInterval.End; x++)
            {
                IntPtr pBackBuffer = bm.BackBuffer;

                pBackBuffer += row * bm.BackBufferStride;
                pBackBuffer += x * 4;

                *((int*)pBackBuffer) = (int)App.Tiles.Air;
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