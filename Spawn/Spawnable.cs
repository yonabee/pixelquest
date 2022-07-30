using Godot;
using static GameUtils;
using System;

public class Spawnable : KinematicBody2D
{

    [Signal]
	public delegate void Despawn(Spawnable entity);

    private CollisionShape2D _collisionShape;
    public CollisionShape2D CollisionShape {
		get { 
			if (_collisionShape == null) {
				_collisionShape = GetNode<CollisionShape2D>("./CollisionShape2D");
			}
			return _collisionShape;
		}
	}

    public bool isFrozen = false;

    public virtual void Spawn() {
        // Spawn logic
    }

    public virtual void DoUpdate() {
        // AI logic
    }

    public virtual void DoFastUpdate() {
        // Realtime AI
    }

    public void Freeze()
    {
        Hide();
        isFrozen = true;
    }

    public void Unfreeze()
    {
        Show();
        isFrozen = false;
    }
}
