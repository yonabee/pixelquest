using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class Spawner : Node2D
{
    [Export]
    public int maxEntities = 500;

    [Export]
    public string entityPath = "res://Entities/Mob/Skeleton.tscn";

    [Export]
    public string entityName = "Skeleton";

    [Export]
    public string rootPath = "/root/Root";

    [Signal]
	public delegate void SpawnCountUpdated(string entityName, int entityCount);

    protected int totalCount = 0;
    protected PackedScene entitySource; 
    protected List<Vector2> spawns = new List<Vector2>();
    protected List<Spawnable> awake = new List<Spawnable>();
    protected List<Spawnable> sleeping = new List<Spawnable>();
    protected Node2D root;

    public override void _Ready()
    {
        entitySource = ResourceLoader.Load(entityPath) as PackedScene;
        root = GetNode(rootPath) as Node2D;

        this.Connect("SpawnCountUpdated", GetNode<CanvasLayer>("../../GUI"), "OnSpawnCountUpdated");
    } 

    public void OnDespawn(Spawnable entity)
    {
        awake.Remove(entity);
        entity.SetProcess(false);
        entity.SetPhysicsProcess(false);
        entity.CollisionShape.Disabled = true;
        sleeping.Add(entity);
        EmitSignal("SpawnCountUpdated", entityName, awake.Count);
    }

    protected void AwakeEntity(Vector2 tile) 
    {
        Spawnable entity;

        if (entitySource != null) {
            if (sleeping.Count> 0) {
                entity = sleeping[0];
                sleeping.Remove(entity);
            } else {
                entity = entitySource.Instance<Spawnable>();
                root.AddChild(entity);
                entity.Connect("Despawn", this, "OnDespawn");
            }
            entity.Position = new Vector2(
                tile.x * 32 + RNG.RandfRange(0, 32),
                tile.y * 32 + RNG.RandfRange(0, 32)
            );
            entity.SetProcess(true);
            entity.SetPhysicsProcess(true);
            entity.CollisionShape.Disabled = false;
            entity.Spawn();

            awake.Add(entity);
            totalCount++;
            EmitSignal("SpawnCountUpdated", entityName, awake.Count);
        }
    }
}
