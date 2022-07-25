using Godot;
using static GameUtils;
using System;

public class Spawnable : KinematicBody2D
{

    [Signal]
	public delegate void Despawn(Spawnable entity);

    public bool isFrozen = false;

    public virtual void Spawn() {
        // Spawn logic
    }

    public virtual void DoUpdate() {
        // AI logic
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
