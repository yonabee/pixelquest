using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class Spawner : Node2D
{
    [Export]
    public int maxEntities = 500;

    [Export]
    public int startEntities = 10;

    [Export]
    public string entityPath = "res://Entities/Mob/Skeleton.tscn";

    private int entityCount = 0;
    private PackedScene entity; 
    private List<Vector2> spawns = new List<Vector2>();
    private List<Spawnable> entities = new List<Spawnable>();
    private List<Spawnable> sleeping = new List<Spawnable>();

    public override void _Ready()
    {
        entity = ResourceLoader.Load(entityPath) as PackedScene;
    } 

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
            InstanceEntity(spawn);
            if (entities.Count == startEntities) {
                break;
            }
        }
    }

    public void _on_Timer_timeout()
    {
        if (entities.Count < maxEntities) {
            foreach(Vector2 spawn in spawns) {
                InstanceEntity(spawn);
                if (entities.Count >= maxEntities) {
                    break;
                }
            }
        }

        foreach(Spawnable entity in entities) {
            entity.UpdateEntity();
        }
    }

    public void OnDespawn(Spawnable entity)
    {
        entities.Remove(entity);
        sleeping.Add(entity);
    }

    private void InstanceEntity(Vector2 tile) 
    {
        Spawnable instance;

        if (entity != null) {
            if (sleeping.Count > 0) {
                instance = sleeping[0];
                sleeping.RemoveAt(0);
            } else {
                instance = entity.Instance<Spawnable>();
                AddChild(instance);
                instance.Connect("Despawn", this, "OnDespawn");
            }
            instance.Position = new Vector2(
                tile.x * 32 + RNG.RandfRange(0, 32),
                tile.y * 32 + RNG.RandfRange(0, 32)
            );
            instance.Spawn();
            entities.Add(instance);
        }
    }
}
