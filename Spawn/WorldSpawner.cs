using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class WorldSpawner : Spawner
{
    [Export]
    public int spawnRate = 10;

    [Export]
    public int totalEntities = 1500;

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
        Vector2 spawn = spawns[RNG.RandiRange(0, spawns.Count - 1)];
        AwakeEntity(spawn);
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
}
