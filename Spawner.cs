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
    public int totalEntities = 1500;

    [Export]
    public string entityPath = "res://Entities/Mob/Skeleton.tscn";

    [Export]
    public string rootPath = "/root/Root";

    private int totalCount = 0;
    private PackedScene entitySource; 
    private List<Vector2> spawns = new List<Vector2>();
    private List<Spawnable> awake = new List<Spawnable>();
    private List<Spawnable> sleeping = new List<Spawnable>();
    private Node2D root;
    private Timer _timer;

    private Timer Timer {
		get { 
			if (_timer == null) {
				_timer = GetNode<Timer>("./Timer"); 
			}
			return _timer; 
		}
	}

    public override void _Ready()
    {
        entitySource = ResourceLoader.Load(entityPath) as PackedScene;
        root = GetNode(rootPath) as Node2D;
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
            AwakeEntity(spawn);
            if (awake.Count == startEntities) {
                break;
            }
        }
        Timer.Start();
    }

    public void _on_Timer_timeout()
    {
        if (awake.Count < maxEntities) {
            foreach(Vector2 spawn in spawns) {
                AwakeEntity(spawn);
                if (awake.Count >= maxEntities) {
                    break;
                }
            }
        }

        for (int i = 0; i < awake.Count; i++) {
            Spawnable entity = awake[i];
            if (entity != null) {
                entity.DoUpdate();
            }
        }
    }

    public void OnDespawn(Spawnable entity)
    {
        awake.Remove(entity);
        sleeping.Add(entity);
    }

    private void AwakeEntity(Vector2 tile) 
    {
        Spawnable instance;

        if (entitySource != null) {
            if (sleeping.Count> 0) {
                instance = sleeping[0];
                sleeping.Remove(instance);
            } else {
                instance = entitySource.Instance<Spawnable>();
                root.AddChild(instance);
                instance.Connect("Despawn", this, "OnDespawn");
            }
            instance.Position = new Vector2(
                tile.x * 32 + RNG.RandfRange(0, 32),
                tile.y * 32 + RNG.RandfRange(0, 32)
            );
            instance.Spawn();

            awake.Add(instance);
            totalCount++;
        }
    }
}
