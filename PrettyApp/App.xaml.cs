using System.Configuration;
using System.Data;
using System.Windows;
using PrettyApp.drawable;
using PrettyApp.plants;

namespace PrettyApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public const double Zoom = 12;
    public readonly List<Plant> PlantList = new();
    private List<(int, int, int)> tiles;

    public enum Tiles
    {
        Dirt = 0x820000,
        Stone = 0x404040,
        Air = 0x38D6D9,
        Wood = 0x4D2528,
        Leaves = 0x2E662E,
        Grass = 0x03FC30
    }

    public void PrepareSimulation()
    {
        // tiles = GenerateTerrain();
        tiles = new();
        for (int i = 0; i < PrettyApp.MainWindow.bm.PixelWidth; i++)
        {
            for (int j = 0; j < PrettyApp.MainWindow.bm.PixelHeight; j++)
            {
                    tiles.Add((i, j, (int)Tiles.Air));
            }
        }

        Plant p = new Plant(
            PrettyApp.MainWindow.bm.PixelWidth / 2,
            (int)(PrettyApp.MainWindow.bm.PixelHeight * 0.85));
        p.Parts.Add(new Seed(p));
        PlantList.Add(p);
    }

    public void RunSimulation()
    {
        RunInBackground(TimeSpan.FromMilliseconds(1000.0/60), () =>
        {
            foreach (Plant plant in PlantList)
            {
                plant.TickParts();
            }

            PrettyApp.MainWindow.DrawPixels([new EmptyPixelable(tiles)]);
            PrettyApp.MainWindow.DrawPixels(PlantList);
        });
        
        RunInBackground(TimeSpan.FromMilliseconds(500.0), () =>
        {
            foreach (Plant plant in PlantList)
            {
                plant.TickPartsSecond();
                PrettyApp.MainWindow.DrawPixels(PlantList);
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