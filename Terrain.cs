using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public enum TerrainType {
    WATER,
    SHORE,
    GRASS,
    SAND,
    CLIFF
}

public class Terrain : TileMap
{
    private TileMap Sand {
    get { return GetChild<TileMap>(0); }
}
    private OpenSimplexNoise noise = new OpenSimplexNoise();

    [Signal]
	public delegate void WorldCreated();

    public override void _Ready()
    {
        this.Connect("WorldCreated", GetNode<KinematicBody2D>("../Player"), "OnWorldCreated");
        this.Connect("WorldCreated", GetNode<TileMap>("../Objects"), "OnWorldCreated");
        this.Connect("WorldCreated", GetNode<WorldSpawner>("../Player/Spawner"), "OnWorldCreated");

        noise.Seed = RandomInt();
        noise.Octaves = 3;
        noise.Period = 16f + (16f * RandomFloat());
        noise.Lacunarity = 4f * RandomFloat();
        noise.Persistence = 0.5f + 0.5f * RandomFloat();

        // Create the earth.
        for(int x = 0; x < 64; x++) {
            for (int y = 0; y < 64; y++) {
                Vector2 loc = new Vector2(x, y);

                if (x > 4 && x <= 60 && y > 4 && y <= 60) {
                    float n = GetNoise(x, y, noise);
                    var terrainType = n <= 0.5f ? TerrainType.WATER : TerrainType.GRASS;
                    TerrainLookup.Add(loc, terrainType);
                } else {
                    if (!TerrainLookup.ContainsKey(loc)) {
                        TerrainLookup.Add(loc, TerrainType.WATER);
                    }
                } 
            }
        }

        // Determine islands.
        Dictionary<Vector2, bool> filled = new Dictionary<Vector2, bool>();
        List<List<Vector2>> islands = new List<List<Vector2>>();
        for(int x = 0; x < 64; x++) {
            for (int y = 0; y < 64; y++) {
                Vector2 loc = new Vector2(x, y);
                filled.Add(loc, false);
            }
        }
        for(int x = 0; x < 64; x++) {
            for (int y = 0; y < 64; y++) {
                Vector2 loc = new Vector2(x, y);  
                if (TerrainLookup.TryGetValue(loc, out TerrainType type)) {
                    if (type == TerrainType.GRASS ) {
                        if (!filled[loc]) {
                            List<Vector2> area = new List<Vector2>();
                            islands.Add(area);
                            FloodFill(loc, area, filled);
                        }
                    } else {
                        filled[loc] = true;
                    }
                }  else {
                    filled[loc] = true;
                }
            }
        }

        // Build bridges.
        if (islands.Count > 1) {
            foreach(List<Vector2> island in islands) 
            {
                int idx = RNG.RandiRange(0, islands.Count - 2);
                if (idx >= islands.IndexOf(island)) {
                    idx++;
                }
                List<Vector2> nextIsland = islands[idx];
                Vector2 start = island[RNG.RandiRange(0, island.Count - 1)];
                Vector2 end = nextIsland[RNG.RandiRange(0, nextIsland.Count - 1)];
                int startX = Math.Min(Mathf.FloorToInt(start.x), Mathf.FloorToInt(end.x));
                int endX = Math.Max(Mathf.FloorToInt(start.x), Mathf.FloorToInt(end.x));
                int startY = Math.Min(Mathf.FloorToInt(start.y), Mathf.FloorToInt(end.y));
                int endY = Math.Max(Mathf.FloorToInt(start.y), Mathf.FloorToInt(end.y));

                int currentX = startX;
                int currentY = startY;
                for (int x = startX; x <= endX; x++) {
                    for (int y = startY; y <= endY; y++) {
                        Vector2 loc = new Vector2(currentX, currentY);
                        TerrainLookup[loc] = TerrainType.GRASS;
                        if (currentX == endX) {
                            currentY++;
                        } else if (currentY == endY) {
                            currentX++;
                        } else if (RandomFloat() > 0.5f) {
                            currentX++;
                        } else {
                            currentY++;
                        }
                    }
                }
            }
        }


        // Mark shorelines.
        for(int x = 0; x < 64; x++) {
            for (int y = 0; y < 64; y++) {   
                Vector2 loc = new Vector2(x, y);  
                if (TerrainLookup.TryGetValue(loc, out TerrainType type)) {
                    if (type == TerrainType.GRASS) {
                        for (int x1 = x - 1; x1 <= x + 1; x1++) {
                            for (int y1 = y - 1; y1 <= y + 1; y1++) {
                                if (x1 == x && y1 == y) {
                                    continue;
                                }
                                Vector2 loc1 = new Vector2(x1, y1);
                                if (TerrainLookup.ContainsKey(loc1)) {
                                    if (TerrainLookup[loc1] == TerrainType.WATER) {
                                        type = TerrainType.SHORE;
                                        break;
                                    }
                                } 
                            }
                            if (type == TerrainType.SHORE) {
                                TerrainLookup[loc] = type;
                                break;
                            }
                        }
                        // Dump sand.
                        if (type == TerrainType.GRASS) {
                            float n = GetNoise(x, y, noise);
                            if (n > 0.6) {
                                TerrainLookup[loc] = TerrainType.SAND;
                                type = TerrainType.SAND;
                            }
                        }
                    } 

                    // Rake the sand.
                    if (type == TerrainType.SAND) {
                        for (int x1 = x - 1; x1 <= x + 1; x1++) {
                            for (int y1 = y - 1; y1 <= y + 1; y1++) {
                                if (x1 == x && y1 == y) {
                                    continue;
                                }
                                Vector2 loc1 = new Vector2(x1, y1);
                                if (TerrainLookup.ContainsKey(loc1)) {
                                    if (TerrainLookup[loc1] == TerrainType.GRASS || TerrainLookup[loc1] == TerrainType.SHORE) {
                                        type = TerrainType.CLIFF;
                                        break;
                                    }
                                } 
                            }
                            if (type == TerrainType.CLIFF) {
                                TerrainLookup[loc] = type;
                                break;
                            }
                        }
                    }
                }  
            }
        }

        if (!TerrainLookup.ContainsValue(TerrainType.SAND)) {
            for (int x = 29; x <= 35; x++) {
                for (int y = 29; y <= 35; y++) {
                    Vector2 loc = new Vector2(x, y);
                    TerrainLookup[loc] = TerrainType.GRASS;
                }
            }

            for (int x = 31; x <= 33; x++) {
                for (int y = 31; y <= 33; y++) {
                    Vector2 loc = new Vector2(x, y);
                    TerrainLookup[loc] = TerrainType.CLIFF;
                }
            }

            TerrainLookup[new Vector2(32,32)] = TerrainType.SAND;

            for (int x = 29; x <= 35; x++) {
                for (int y = 29; y <= 35; y++) {
                    if (x < 30 || x > 34 || y < 30 || y > 34) {
                        Vector2 loc = new Vector2(x, y);
                        for (int x1 = x - 1; x1 <= x + 1; x1++) {
                            for (int y1 = y - 1; y1 <= y + 1; y1++) {
                                if (x1 == x && y1 == y) {
                                    continue;
                                }
                                Vector2 loc1 = new Vector2(x1, y1);
                                if (TerrainLookup[loc1] == TerrainType.WATER) {
                                    TerrainLookup[loc] = TerrainType.SHORE;
                                    break;
                                }
                            }
                            if (TerrainLookup[loc] == TerrainType.SHORE) {
                                break;
                            }
                        }
                    }
                    
                }
            }
        }

        // Now the tilemap cells can be assigned.
        var tiles = new Dictionary<string, int>();
        foreach(int id in TileSet.GetTilesIds()) {
            tiles.Add(TileSet.TileGetName(id), id);
        }
        for(int x = 0; x <= 64; x++) {
            for (int y = 0; y <= 64; y++) {
                if (TerrainLookup.TryGetValue(new Vector2(x,y), out TerrainType type)) {
                    Vector2 loc = new Vector2(x,y);
                    switch(type) {
                        case TerrainType.WATER:
                            SetCellv(loc, tiles["Water"]);
                            break;
                        case TerrainType.SHORE:
                            SetCellv(loc, tiles["Shore"]);
                            break;
                        case TerrainType.GRASS:
                            SetCellv(loc, tiles["Shore"]);
                            break;
                        case TerrainType.CLIFF:
                            SetCellv(loc, tiles["Shore"]);
                            Sand.SetCellv(loc, tiles["Cliff"]);
                            break;
                        case TerrainType.SAND:
                            SetCellv(loc, tiles["Shore"]);
                            Sand.SetCellv(loc, tiles["Cliff"]);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        UpdateBitmaskRegion(new Vector2(-64,-64), new Vector2(64,64));
        Sand.UpdateBitmaskRegion(new Vector2(-64,-64), new Vector2(64,64));
        EmitSignal("WorldCreated");
    }

    private void FloodFill(Vector2 loc, List<Vector2> island, Dictionary<Vector2, bool> filled)
    {
        filled[loc] = true;
        island.Add(loc);
        for (int x = Mathf.FloorToInt(loc.x) - 1; x <= Mathf.FloorToInt(loc.x) + 1; x++) {
            for (int y = Mathf.FloorToInt(loc.y) - 1; y <= Mathf.FloorToInt(loc.y) + 1; y++) {
                if (x < 0 || x >= 64 || y < 0 || y >= 64) {
                    continue;
                }
                if (x == loc.x && y == loc.y) {
                    continue;
                }
                Vector2 loc1 = new Vector2(x, y);
                if (filled[loc1]) {
                    continue;
                }

                if (TerrainLookup.TryGetValue(loc1, out TerrainType type)) {
                    if (type == TerrainType.GRASS) {
                        FloodFill(loc1, island, filled);
                    } else {
                        filled[loc1] = true;
                    }
                } else {
                    filled[loc1] = true;
                }
            }
        }
    }
}
