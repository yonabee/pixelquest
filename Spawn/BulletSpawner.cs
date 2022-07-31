using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class BulletSpawner : Spawner<Bullet>
{
    [Export]
    public int timeToLive = 120;

    [Signal]
	public delegate void SpawnCountUpdated(string entityName, int entityCount);

    private Timer _updateTimer;
    private Timer UpdateTimer {
		get { 
			if (_updateTimer == null) {
				_updateTimer = GetNode<Timer>("./Update Timer"); 
			}
			return _updateTimer; 
		}
	}

    public override void _Ready()
    {
        base._Ready();
        UpdateTimer.Start();
    }

    public override void OnDespawn(Bullet entity) {
        entity.CollisionShape.Disabled = true;
        entity.Visible = false;
        base.OnDespawn(entity);
    }

    public override Bullet AwakeEntity(Vector2 position)
    {
        Bullet entity = base.AwakeEntity(position);
        entity.Birthmark = OS.GetSystemTimeSecs();
        entity.CollisionShape.Disabled = false;
        entity.Visible = true;
        entity.Spawn();
        return entity;
    }

    public void _on_Timer_timeout()
    {
        for (int i = 0; i < awake.Count; i++) {
            Bullet entity = awake[i];
            if (entity != null) {
                if (entity.Birthmark + (ulong)timeToLive < OS.GetSystemTimeSecs()) {
                    OnDespawn(entity);
                }
            }
        }
    }

}
