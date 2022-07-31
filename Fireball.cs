using Godot;
using System;

public class Fireball : Bullet
{
    [Export]
    public int speed = 160;

    public Vector2 direction;

    private AnimatedSprite _sprite;
    private AnimatedSprite Sprite {
		get { 
			if (_sprite == null) {
				_sprite = GetNode<AnimatedSprite>("./AnimatedSprite");
			}
			return _sprite;
		}
	}

    public override void _Process(float delta)
    {
        Position = Position + speed * delta * direction;
    }

    public void _on_Fireball_body_entered(Node body)
    {
        if (body.Name == "Player") {
            return;
        }
        if (body.Name == "Terrain") {
            return;
        }
        if (body.Name.Contains("Skeleton")) {
            (body as Skeleton).Hit(attackDamage);
        }

        direction = Vector2.Zero;
        Sprite.Play("explode");
    }

    public void _on_AnimatedSprite_animation_finished()
    {
        if (Sprite.Animation == "explode") {
            EmitSignal("Despawn", this);
            Sprite.Play("fly");
        }
    }
}
