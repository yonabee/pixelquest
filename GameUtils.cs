using Godot;
using System;
using System.Collections.Generic;

public static class GameUtils
{
    private static int[] autoTileBitmask = {
        0,0,0, 0,0,0, 0,0,0, 0,0,0, 1,1,0, 0,0,0, 0,0,0, 0,1,1, 0,0,0, 0,1,0, 0,0,0, 0,0,0,
        0,1,0, 0,1,1, 1,1,1, 1,1,0, 1,1,1, 1,1,1, 1,1,1, 1,1,1, 0,1,1, 1,1,1, 1,1,1, 1,1,0,
        0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,1, 1,1,0, 0,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0,

        0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,1, 1,1,0, 0,1,0, 0,1,1, 0,1,1, 0,0,0, 1,1,0,
        0,1,0, 0,1,1, 1,1,1, 1,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0, 0,1,1, 1,1,1, 0,0,0, 1,1,1,
        0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0, 0,1,1, 1,1,0, 0,0,0, 1,1,0,

        0,1,0, 0,1,0, 0,1,0, 0,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0, 0,1,1, 1,1,1, 1,1,0, 1,1,0,
        0,1,0, 0,1,1, 1,1,1, 1,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0, 1,1,1, 1,1,1, 1,1,1, 1,1,0,
        0,0,0, 0,0,0, 0,0,0, 0,0,0, 0,1,0, 0,1,1, 1,1,0, 0,1,0, 0,1,1, 1,1,1, 0,1,1, 1,1,0,

        0,0,0, 0,0,0, 0,0,0, 0,0,0, 0,1,0, 0,1,1, 1,1,0, 0,1,0, 0,1,1, 1,1,1, 1,1,1, 1,1,0,
        0,1,0, 0,1,1, 1,1,1, 1,1,0, 1,1,1, 1,1,1, 1,1,1, 1,1,1, 0,1,1, 1,1,1, 1,1,1, 1,1,0,
        0,0,0, 0,0,0, 0,0,0, 0,0,0, 1,1,0, 0,0,0, 0,0,0, 0,1,1, 0,0,0, 0,0,0, 0,1,0, 0,0,0
    };
    private static Random random = new Random(Guid.NewGuid().GetHashCode());
    private static RandomNumberGenerator _rng;
    public static RandomNumberGenerator RNG {
        get {
            if (_rng == null) {
                _rng = new RandomNumberGenerator();
                _rng.Seed = (ulong)Guid.NewGuid().GetHashCode();
            }
            return _rng;
        }
    }

    public static Dictionary<Vector2, TerrainType> TerrainLookup = new Dictionary<Vector2, TerrainType>();

    public static string GetGridDirection(Vector2 direction)
	{
		var norm = direction.Normalized();
		if (norm.y >= 0.707) {
			return "down";
		} else if (norm.y <= -0.707) {
			return "up";
		} else if (norm.x <= -0.707) {
			return "left";
		} else if (norm.x >= 0.707) {
			return "right";
		}
		return "down";
	}

    public static int RandomInt()
    {
        return (int)RNG.Randi();
    }

    public static float RandomFloat()
    {
        return RNG.Randf();
    }

    public static int ScaleToInt(float input, int scale) 
    {
        return Mathf.FloorToInt(input * (scale + 1));
    }

    public static float GetNoise(int x, int y, OpenSimplexNoise noise) {
        float n = noise.GetNoise2d(x, y);
        return (n + 1.0f) / 2.0f;
    }

    public static float GetNoise(float x, float y, OpenSimplexNoise noise) {
        float n = noise.GetNoise2d(x, y);
        return (n + 1.0f) / 2.0f;
    }

    public static bool AutoTileBitmask(int x, int y) {
        if (autoTileBitmask[y*36 + x] == 1) {
            return true;
        }
        return false;
    }
}
