using Godot;
using static GameUtils;
using System;

public class Spawnable : KinematicBody2D
{

    [Signal]
	public delegate void Despawn(Spawnable entity);

    public virtual void Spawn() {
        // Spawn logic
    }

    public virtual void UpdateEntity() {
        // AI logic
    }
}
