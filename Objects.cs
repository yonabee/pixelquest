using Godot;
using static GameUtils;
using System;

public class Objects : TileMap
{
    private OpenSimplexNoise noise = new OpenSimplexNoise();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        noise.Seed = RandomInt();
        noise.Octaves = 3;
        noise.Period = 16f * RandomFloat();
        noise.Lacunarity = 4f * RandomFloat();
        noise.Persistence = 0.5f + 0.5f * RandomFloat();
    }

    public void OnWorldCreated() 
    {
        for(int x = -128; x <= 128; x++) {
            for (int y = -128; y <= 128; y++) {
                Vector2 loc = new Vector2(Mathf.Floor(x/2) - 1, Mathf.Floor(y/2));
                if (TerrainLookup.TryGetValue(loc, out TerrainType type)) {
                    if (type == TerrainType.GRASS) {
                        float n = GetNoise(x, y, noise); 
                        int idx = Mathf.FloorToInt(n * 7);
                        SetCell(x, y, 1);
                    }
                }
            }
        }
    }
}
