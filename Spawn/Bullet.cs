using Godot;
using static GameUtils;
using System;

public class Bullet : Area2D
{
    [Signal]
	public delegate void Despawn(Bullet entity);

    public ulong Birthmark { get; set; }

    public int attackDamage;

    private CollisionShape2D _collisionShape;
    public CollisionShape2D CollisionShape {
		get { 
			if (_collisionShape == null) {
				_collisionShape = GetNode<CollisionShape2D>("./CollisionShape2D");
			}
			return _collisionShape;
		}
	}

    private TileMap _tilemap;
    public TileMap Terrain { get { return _tilemap; }}

    public override void _Ready()
    {
        _tilemap = GetTree().Root.GetNode<TileMap>("Root/Terrain");
    }

    public virtual void Spawn() {
        // Spawn logic
    }
}
