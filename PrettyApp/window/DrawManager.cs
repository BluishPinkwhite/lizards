using System.Windows;
using System.Windows.Input;
using PrettyApp.drawable;
using PrettyApp.util;

namespace PrettyApp.window;

public static class DrawManager
{
    internal static void DrawSinglePixel(MouseEventArgs e)
    {
        byte[] color = [0, 200, 50];
        Int32Rect rect = new Int32Rect(
            (int)Math.Ceiling(e.GetPosition(MainWindow.image).X / App.Zoom),
            (int)Math.Ceiling(e.GetPosition(MainWindow.image).Y / App.Zoom),
            1, 1);

        MainWindow.bm.WritePixels(rect, color, 4, 0);
    }


    internal static void DrawPixels(List<Entity> entities)
    {
        try
        {
            MainWindow.bm.Lock();

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
                    bounds.ClampToScreen(MainWindow.bm.PixelWidth - 1, MainWindow.bm.PixelHeight - 1);
                    MainWindow.bm.AddDirtyRect(new Int32Rect(bounds.X, bounds.Y, bounds.Width() + 1, bounds.Height() + 1));

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
                        lastBounds.ClampToScreen(MainWindow.bm.PixelWidth - 1, MainWindow.bm.PixelHeight - 1);
                        MainWindow.bm.AddDirtyRect(new Int32Rect(lastBounds.X, lastBounds.Y, lastBounds.Width() + 1,
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
                    rowIntervals.Sort((interval, interval1) => interval.Start - interval1.Start);

                    int index = 0; // output array index
                    for (int i = 1; i < rowIntervals.Count; i++)
                    {
                        Interval interval = rowIntervals[index];
                        Interval intervalNext = rowIntervals[i];
                        
                        if (interval.End >= intervalNext.Start) 
                        {
                            // merge intervals
                            interval.End = Math.Max(interval.End, intervalNext.End);
                            rowIntervals[index] = interval;
                        }
                        else
                        {
                            // move to next for merging into it
                            index++;
                            rowIntervals[index] = intervalNext;
                        }
                    }

                    // result is [0..index-1]
                    rowsBoundingBoxes[keyValuePair.Key] = rowIntervals.Take(index + 1).ToList();
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
                    Dictionary<int, int> data = entity.GetPixelData();

                    foreach (KeyValuePair<int,int> pixel in data)
                    {
                        int x = pixel.Key & 0x0000FFFF;
                        int y = pixel.Key >> 16;
                        int color = pixel.Value;
                        
                        if (x < 0 || y < 0 || x >= MainWindow.bm.PixelWidth || y >= MainWindow.bm.PixelHeight)
                        {
                            Console.Out.WriteLine($"Pixel outside image: ({x},{y}), {color:X}, skipping...");
                            continue;
                        }

                        IntPtr pBackBuffer = MainWindow.bm.BackBuffer;

                        pBackBuffer += y * MainWindow.bm.BackBufferStride;
                        pBackBuffer += x * 4;

                        *((int*)pBackBuffer) = color;
                    }
                }
            }
        }
        finally
        {
            MainWindow.bm.Unlock();
        }
    }

    
    /** 
     * CALL ONLY WHEN WritableBitmap IS LOCKED
     */
    private static void ResetBackground(BoundingBox bounds)
    {
        unsafe
        {
            // TODO: block by block cache processing?

            for (int y = bounds.Y; y <= bounds.Ey; y++)
            {
                for (int x = bounds.X; x <= bounds.Ex; x++)
                {
                    IntPtr pBackBuffer = MainWindow.bm.BackBuffer;

                    pBackBuffer += y * MainWindow.bm.BackBufferStride;
                    pBackBuffer += x * 4;

                    *((int*)pBackBuffer) = (int)App.Tiles.Air;
                }
            }
        }
    }
    
    
    /**
     * CALL ONLY WHEN WritableBitmap IS LOCKED
     */
    private static void ResetRow(int row, Interval columnInterval)
    {
        unsafe
        {
            // TODO: block by block cache processing?

            for (int x = columnInterval.Start; x <= columnInterval.End; x++)
            {
                IntPtr pBackBuffer = MainWindow.bm.BackBuffer;

                pBackBuffer += row * MainWindow.bm.BackBufferStride;
                pBackBuffer += x * 4;

                *((int*)pBackBuffer) = (int)App.Tiles.Air;
            }
        }
    }

    internal static void ClearRenderedScene()
    {
        try
        {
            MainWindow.bm.Lock();

            ResetBackground(new BoundingBox(0, 0, MainWindow.bm.PixelWidth - 1, MainWindow.bm.PixelHeight - 1));

            MainWindow.bm.AddDirtyRect(new Int32Rect(0, 0, MainWindow.bm.PixelWidth, MainWindow.bm.PixelHeight));
        }
        finally
        {
            MainWindow.bm.Unlock();
        }
    }
}