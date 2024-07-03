using System.Numerics;
using System.Windows;
using PrettyApp.drawable;
using PrettyApp.lizard;
using PrettyApp.util;
using Point = System.Drawing.Point;

namespace PrettyApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public const double Zoom = 2.5;
    public static readonly List<Entity> Entities = new();
    private List<Pixel> tiles;

    public enum Tiles
    {
        Dirt = 0x820000,
        Stone = 0x404040,
        Air = 0x101010,//0x38D6D9,
        Wood = 0x4D2528,
        Leaves = 0x2E662E,
        Grass = 0x03FC30
    }

    public void PrepareSimulation()
    {
        // LineEntity line = new LineEntity(new Point(PrettyApp.MainWindow.bm.PixelWidth / 2, PrettyApp.MainWindow.bm.PixelHeight / 2));
        // Entities.Add(line);


        try
        {
            LizardEntity l = new LizardEntity(
                new Point(PrettyApp.MainWindow.bm.PixelWidth / 2, PrettyApp.MainWindow.bm.PixelHeight / 2));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }
    }

    public void RunSimulation()
    {
        RunInBackground(TimeSpan.FromMilliseconds(1000.0 / 30), () =>
        {
            try
            {
                foreach (Entity entity in Entities)
                {
                    entity.Tick();
                    entity.UpdatePixelData();
                }

                PrettyApp.MainWindow.DrawPixels(Entities);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        });

        RunInBackground(TimeSpan.FromMilliseconds(125.0), () =>
        {
            try
            {
                foreach (Entity entity in Entities)
                {
                    entity.TickSecond();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        });
    }

    async Task RunInBackground(TimeSpan timeSpan, Action action)
    {
        var periodicTimer = new PeriodicTimer(timeSpan);
        while (await periodicTimer.WaitForNextTickAsync())
        {
            action();
        }
    }


    private List<(int, int, int)> GenerateTerrain()
    {
        double[] coefficients = new double[8];
        Random r = new Random(100);

        for (int i = 0; i < coefficients.Length; i++)
        {
            coefficients[i] = r.NextDouble() / Zoom;
        }

        List<(int, int, int)> tiles = new List<(int, int, int)>();

        for (int i = 0; i < PrettyApp.MainWindow.bm.PixelWidth; i++)
        {
            double terrainHeight = PrettyApp.MainWindow.bm.PixelHeight * 0.9;
            for (int j = 0; j < coefficients.Length; j++)
            {
                terrainHeight += Math.Sin(i * j) * coefficients[j];
            }

            int depth = (int)terrainHeight;

            for (int j = 0; j < PrettyApp.MainWindow.bm.PixelHeight; j++)
            {
                int color;
                if (j == depth)
                {
                    color = (int)Tiles.Grass;
                }
                else if (j > depth)
                {
                    if (j > depth + 5)
                    {
                        color = (int)Tiles.Stone;
                    }
                    else
                    {
                        color = (int)Tiles.Dirt;
                    }
                }
                else
                {
                    color = (int)Tiles.Air;
                }


                tiles.Add((i, j, color));
            }
        }

        return tiles;
    }
}