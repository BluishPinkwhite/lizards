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
                //      - combined into single 32bit int: [31-16] start, [15-0] end
                Dictionary<int, List<int>> rowsBoundingBoxes = new Dictionary<int, List<int>>();

                // add in-bounding-box rows of updated entities to dictionary
                foreach (Entity entity in entities)
                {
                    if (entity.HasJustUpdated)
                    {
                        entity.HasJustUpdated = false;
                        
                        BoundingBox bounds = entity.GetBoundingBox();
                        bounds.ClampToScreen(MainWindow.bm.PixelWidth - 1, MainWindow.bm.PixelHeight - 1);
                        
                        BoundingBox lastBounds = entity.GetLastBoundingBox();
                        lastBounds.ClampToScreen(MainWindow.bm.PixelWidth - 1, MainWindow.bm.PixelHeight - 1);

                        // it is better to combine the bounding boxes for less drawing
                        BoundingBox combined = Util.CombineBoundingBoxes(bounds, lastBounds);
                        if (bounds.Area() + lastBounds.Area() > combined.Area())
                        {
                            MainWindow.bm.AddDirtyRect(combined.ToRect());
                        }
                        // it is not better -> keep the separate ones
                        else
                        {
                            MainWindow.bm.AddDirtyRect(bounds.ToRect());
                            MainWindow.bm.AddDirtyRect(lastBounds.ToRect());
                        }

                        for (int row = lastBounds.Y; row <= lastBounds.Ey; row++)
                        {
                            if (!rowsBoundingBoxes.ContainsKey(row))
                                rowsBoundingBoxes[row] = new List<int>();

                            // adding only lastBounds because "current bounds" will be overwritten anyway by pixel data
                            rowsBoundingBoxes[row].Add(lastBounds.Ex + (lastBounds.X << 16));
                        }
                    }
                }

                // merge all overlapping intervals in rows
                foreach (KeyValuePair<int, List<int>> keyValuePair in rowsBoundingBoxes)
                {
                    List<int> rowIntervals = keyValuePair.Value;
                    rowIntervals.Sort(
                        (interval1, interval2) => (interval1 >> 16) - (interval2 >> 16));

                    int index = 0; // output array index
                    for (int i = 1; i < rowIntervals.Count; i++)
                    {
                        int interval = rowIntervals[index];
                        int intervalNext = rowIntervals[i];

                        int intervalEnd = interval & 0x0000FFFF;
                        int intervalNextEnd = intervalNext & 0x0000FFFF;

                        // if checked end >= next interval start
                        if (intervalEnd >= intervalNext >> 16)
                        {
                            // merge intervals
                            intervalEnd = Math.Max(intervalEnd, intervalNextEnd);
                            int intervalStart = interval >> 16;
                            rowIntervals[index] = intervalEnd + (intervalStart << 16);
                        }
                        else
                        {
                            // move to next for merging into it
                            index++;
                            rowIntervals[index] = intervalNext;
                        }
                    }

                    // result is [0..index]
                    rowsBoundingBoxes[keyValuePair.Key] = rowIntervals.Take(index + 1).ToList();
                }


                // use merged bounding boxes to reset background
                foreach (KeyValuePair<int, List<int>> keyValuePair in rowsBoundingBoxes)
                {
                    int row = keyValuePair.Key;
                    List<int> columns = keyValuePair.Value;

                    foreach (int columnInterval in columns)
                    {
                        ResetRow(row, columnInterval >> 16, columnInterval & 0x0000FFFF);
                    }
                }


                // draw entities over reset background
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    Entity entity = entities[i];
                    Dictionary<int, int> data = entity.GetPixelData();

                    foreach (KeyValuePair<int, int> pixel in data)
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

                        // get correct write location based on coords
                        pBackBuffer += y * MainWindow.bm.BackBufferStride;
                        pBackBuffer += x * 4;

                        // write pixel color
                        *((int*)pBackBuffer) = color;
                    }
                }
            }
        }
        finally
        {
            // allow to show rendered image
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
    private static void ResetRow(int row, int startX, int endX)
    {
        unsafe
        {
            // TODO: block by block cache processing?

            for (int x = startX; x <= endX; x++)
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