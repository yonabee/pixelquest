using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public enum TerrainType {
    WATER,
    SHORE,
    GRASS
}

public class Terrain : TileMap
{
    private OpenSimplexNoise noise = new OpenSimplexNoise();

    [Signal]
	public delegate void WorldCreated();

    public override void _Ready()
    {
        this.Connect("WorldCreated", GetNode<KinematicBody2D>("../Player"), "OnWorldCreated");
        this.Connect("WorldCreated", GetNode<TileMap>("../Objects"), "OnWorldCreated");

        noise.Seed = RandomInt();
        noise.Octaves = 3;
        noise.Period = 1f + (32f * RandomFloat());
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

        for(int x = -64; x < 64; x++) {
            for (int y = -64; y < 64; y++) {

                float n = GetNoise(x, y, noise);
                Vector2 loc = new Vector2(x, y);

                // int tileIdx = FloatToInt(nValue, rows * columns);
                // int tileX = tileIdx % columns;
                // int tileY = Mathf.FloorToInt(tileIdx / columns);
                if (x > -60 && x <= 60 && y > -60 && y <= 60) {
                    
                    if (TerrainLookup.ContainsKey(loc)) {
                        SetCellv(loc, TerrainLookup[loc] == TerrainType.WATER ? 1 : 0);
                    } else {
                        var terrainType = n <= 0.5f ? TerrainType.WATER : TerrainType.GRASS;
                        SetCellv(loc, terrainType == TerrainType.WATER ? 1 : 0);
                        for (int x1 = x - 1; x1 <= x + 1; x1++) {
                            for (int y1 = y - 1; y1 <= y + 1; y1++) {
                                if (x1 == x && y1 == y) {
                                    continue;
                                }
                                Vector2 loc1 = new Vector2(x1, y1);
                                if (TerrainLookup.ContainsKey(loc1)) {
                                    if (TerrainLookup[loc1] == TerrainType.WATER) {
                                        terrainType = TerrainType.SHORE;
                                        break;
                                    }
                                } else {
                                    if (x > -60 && x <= 60 && y > -60 && y <= 60) {
                                        float n1 = GetNoise(x1, y1, noise);
                                        if (n1 <= 0.5f) {
                                            TerrainLookup.Add(loc1, TerrainType.WATER);
                                            terrainType = TerrainType.SHORE;
                                            break;
                                        }
                                    } else {
                                        TerrainLookup.Add(loc1, TerrainType.WATER);
                                        terrainType = TerrainType.SHORE;
                                        break;
                                    }

                                }
                            }
                            if (terrainType == TerrainType.SHORE) {
                                break;
                            }
                        }
                        TerrainLookup.Add(loc, terrainType);
                    }
                } else {
                    SetCellv(loc, 1);
                    if (!TerrainLookup.ContainsKey(loc)) {
                        TerrainLookup.Add(loc, TerrainType.WATER);
                    }
                }         
            }
        }

        UpdateBitmaskRegion(new Vector2(-64,-64), new Vector2(64,64));
        EmitSignal("WorldCreated");
    }
}
