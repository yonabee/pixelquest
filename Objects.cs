using Godot;
using static GameUtils;
using System;

public class Objects : TileMap
{
    private OpenSimplexNoise noise = new OpenSimplexNoise();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        noise.Octaves = 3;
        noise.Period = 64f * RandomFloat();
        noise.Lacunarity = 4f * RandomFloat();
        noise.Persistence = 1f;
        noise.Seed = RandomInt();
    }

    public void OnWorldCreated() 
    {
        for(int x = 0; x < 128; x++) {
            for (int y = 0; y < 128; y++) {
                Vector2 loc = new Vector2(Mathf.Floor(x/2), Mathf.Floor(y/2));
                if (TerrainLookup.TryGetValue(loc, out TerrainType type)) {
                    if (type == TerrainType.GRASS) {
                        float n = GetNoise(x, y, noise); 
                        if ((ScaleToInt(n, 10) % 3) > 0) {
                            if (n < 0.5f) {
                                SetCell(x, y, 0);
                            } else if (n < 0.6f) {
                                SetCell(x, y, 1);
                            } else {
                                SetCell(x, y, 2);
                            }
                            
                        }
                    }
                    if (type == TerrainType.WATER) {
                        SetCell(x, y, 7);
                    }
                }
            }
        }
    }
}
