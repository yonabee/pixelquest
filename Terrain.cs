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
        this.Connect("WorldCreated", GetNode<Spawner>("../Spawner"), "OnWorldCreated");

        noise.Seed = RandomInt();
        noise.Octaves = 3;
        noise.Period = 16f + (16f * RandomFloat());
        noise.Lacunarity = 4f * RandomFloat();
        noise.Persistence = 0.5f + 0.5f * RandomFloat();

        // Texture tex = new ImageTexture();
        // Image blobs = new Image();
        // blobs.Create(192, 64, false, Image.Format.Rgba8);
        // blobs.Lock();
        // blobs.Unlock();

        // TileSet = new TileSet();
        // TileSet.CreateTile(0);
        // TileSet.TileSetTexture(0, tex);

        // Determine main areas of land and water.
        for(int x = 0; x < 64; x++) {
            for (int y = 0; y < 64; y++) {
                Vector2 loc = new Vector2(x, y);

                if (x > 4 && x <= 60 && y > 4 && y <= 60) {
                    if (TerrainLookup.ContainsKey(loc)) {
                        //SetCellv(loc, TerrainLookup[loc] == TerrainType.WATER ? 1 : 0);
                    } else {
                        float n = GetNoise(x, y, noise);
                        var terrainType = n <= 0.5f ? TerrainType.WATER : TerrainType.GRASS;
                        //SetCellv(loc, terrainType == TerrainType.WATER ? 1 : 0);

                        TerrainLookup.Add(loc, terrainType);
                    }
                } else {
                    //SetCellv(loc, 1);
                    if (!TerrainLookup.ContainsKey(loc)) {
                        TerrainLookup.Add(loc, TerrainType.WATER);
                    }
                } 


            }

        }

        // Find and mark edges.
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
                        // Introduce sand.
                        if (type == TerrainType.GRASS) {
                            float n = GetNoise(x, y, noise);
                            if (n > 0.6) {
                                TerrainLookup[loc] = TerrainType.SAND;
                                type = TerrainType.SAND;
                            }
                        }
                    } 
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
}
