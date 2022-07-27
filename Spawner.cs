using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class Spawner : Node2D
{
    [Export]
    public int maxEntities = 500;

    [Export]
    public int spawnRate = 10;

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
    private Timer _updateTimer;
    private Timer _spawnTimer;
    private Timer _fastTimer;

    private Timer UpdateTimer {
		get { 
			if (_updateTimer == null) {
				_updateTimer = GetNode<Timer>("./Update Timer"); 
			}
			return _updateTimer; 
		}
	}

    private Timer SpawnTimer {
		get { 
			if (_spawnTimer == null) {
				_spawnTimer = GetNode<Timer>("./Spawn Timer"); 
			}
			return _spawnTimer; 
		}
	}

    private Timer FastTimer {
		get { 
			if (_fastTimer == null) {
				_fastTimer = GetNode<Timer>("./Fast Timer"); 
			}
			return _fastTimer; 
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
            if (awake.Count == spawnRate) {
                break;
            }
        }
        do {
            Vector2 spawn = spawns[RNG.RandiRange(0, spawns.Count - 1)];
            AwakeEntity(spawn);
        } while (awake.Count < spawnRate);

        UpdateTimer.Start();
        SpawnTimer.Start();
        FastTimer.Start();
    }

    public void _on_Timer_timeout()
    {
        for (int i = 0; i < awake.Count; i++) {
            Spawnable entity = awake[i];
            if (entity != null) {
                entity.DoUpdate();
            }
        }
    }

    public void _on_Spawn_Timer_timeout()
    {
        int cohort = 0;
        if (awake.Count < maxEntities) {
            do {
                Vector2 spawn = spawns[RNG.RandiRange(0, spawns.Count - 1)];
                AwakeEntity(spawn);
                cohort++;
            } while (cohort < spawnRate && awake.Count < maxEntities);
        }
    }

    public void _on_Fast_Timer_timeout()
    {
        for (int i = 0; i < awake.Count; i++) {
            Spawnable entity = awake[i];
            if (entity != null && !entity.isFrozen) {
                entity.DoFastUpdate();
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
