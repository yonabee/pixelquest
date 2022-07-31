using Godot;
using static GameUtils;
using System;
using System.Collections.Generic;

public class Spawner<T> : Node2D where T : Node2D
{
    [Export]
    public int maxEntities = 500;

    [Export]
    public string entityPath = "res://Entities/Mob/Skeleton.tscn";

    [Export]
    public string entityName = "Skeleton";

    [Export]
    public string rootPath = "/root/Root";

    protected int totalCount = 0;
    protected PackedScene entitySource; 
    protected List<Vector2> spawns = new List<Vector2>();
    protected List<T> awake = new List<T>();
    protected List<T> sleeping = new List<T>();
    protected Node2D root;

    public override void _Ready()
    {
        entitySource = ResourceLoader.Load(entityPath) as PackedScene;
        root = GetNode(rootPath) as Node2D;

        this.Connect("SpawnCountUpdated", GetNode<CanvasLayer>("../../GUI"), "OnSpawnCountUpdated");
    } 

    public virtual void OnDespawn(T entity)
    {
        awake.Remove(entity);
        entity.SetProcess(false);
        entity.SetPhysicsProcess(false);
        sleeping.Add(entity);
        EmitSignal("SpawnCountUpdated", entityName, awake.Count);
    }

    public virtual T AwakeEntity(Vector2 position) 
    {
        T entity = null;

        if (entitySource != null) {
            if (sleeping.Count > 0) {
                entity = sleeping[0];
                sleeping.Remove(entity);
            } else {
                entity = entitySource.Instance<T>();
                root.AddChild(entity);
                entity.Connect("Despawn", this, "OnDespawn");
            }
            entity.Position = position;
            entity.SetProcess(true);
            entity.SetPhysicsProcess(true);

            awake.Add(entity);
            totalCount++;
            EmitSignal("SpawnCountUpdated", entityName, awake.Count);
        }

        return entity;
    }
}
