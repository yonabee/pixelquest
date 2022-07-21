using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class Spawner : Node2D
{
    [Export]
    public int maxSkeletons = 40;

    [Export]
    public int startSkeletons = 10;

    private int skeletonCount = 0;
    private PackedScene skeletonScene = ResourceLoader.Load("res://Entities/Mob/Skeleton.tscn") as PackedScene;

    private List<Vector2> spawns = new List<Vector2>();

    public void OnWorldCreated()
    {
        for(int x = 0; x < 64; x++) {
            for(int y = 0; y < 64; y++) {
                Vector2 loc = new Vector2(x,y);
                if (TerrainLookup.TryGetValue(loc, out TerrainType type)) {
                    if (type == TerrainType.CLIFF) {
                        spawns.Add(loc);
                    }
                }
            }
        } 
        foreach(Vector2 spawn in spawns) {
            InstanceSkeleton(spawn);
            if (skeletonCount == startSkeletons) {
                break;
            }
        }
    }

    public void _on_Timer_timeout()
    {
        if (skeletonCount < maxSkeletons) {
            foreach(Vector2 spawn in spawns) {
                InstanceSkeleton(spawn);
                if (skeletonCount == maxSkeletons * spawns.Count) {
                    break;
                }
            }
        }
    }

    public void OnSkeletonDeath()
    {
        skeletonCount -= skeletonCount;
    }

    private void InstanceSkeleton(Vector2 tile) 
    {
        var skeleton = skeletonScene.Instance<Skeleton>();
        AddChild(skeleton);
        skeleton.Position = new Vector2(
            tile.x * 32 + RNG.RandfRange(0, 32),
            tile.y * 32 + RNG.RandfRange(0, 32)
        );
        skeleton.Connect("Death", this, "OnSkeletonDeath");
        skeleton.Arise();
        skeletonCount += 1;
    }
}
