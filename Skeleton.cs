using Godot;
using static GameUtils;
using System;

public class Skeleton : Spawnable
{
    [Export]
    public int speed = 25;

    [Export]
    public int health = 50;

    [Export]
    public int healthMax = 50;

    [Export]
    public int healthRegeneration = 1;

    private Vector2 direction;
    private Vector2 lastDirection = new Vector2(0,1);
    private int bounceCountdown = 0;
    private bool animationPlaying = false;
    private Player _player;
    private Player Player {
		get 
        {
            if (_player == null) {
                _player = GetTree().Root.GetNode<Player>("Root/Player");
            }
            return _player;
        }
	}

    private AnimatedSprite _sprite;
    private AnimatedSprite Sprite {
		get { 
			if (_sprite == null) {
				_sprite = GetNode<AnimatedSprite>("./AnimatedSprite");
			}
			return _sprite;
		}
	}

    private AnimationPlayer _animationPlayer;
    private AnimationPlayer AnimationPlayer {
		get { 
			if (_animationPlayer == null) {
				_animationPlayer = GetNode<AnimationPlayer>("./AnimationPlayer");
			}
			return _animationPlayer;
		}
	}


    private OpenSimplexNoise noise = new OpenSimplexNoise();

    public override void _Ready()
    {
        noise.Seed = RandomInt();
        noise.Octaves = 1;
        noise.Period = 12f * RandomFloat();
        noise.Lacunarity = 4f * RandomFloat();
        noise.Persistence = 1f * RandomFloat();
    }

    public override void _Process(float delta)
    {
        health = Mathf.FloorToInt(Mathf.Min(health + healthRegeneration * delta, healthMax));
    }

    public override void _PhysicsProcess(float delta)
    {
        Vector2 movement = direction * speed * delta;
        KinematicCollision2D collision = MoveAndCollide(movement);
        if (collision != null && (collision.Collider as Node).Name != "Player") {
            direction = direction.Rotated(RNG.RandfRange(Mathf.Pi / 4, Mathf.Pi / 2));
            bounceCountdown = RNG.RandiRange(2, 5);
        }
        if (!animationPlaying) {
            AnimateMonster(direction);
        }
    }

    public override void UpdateEntity() 
    {
        Vector2 playerRelativePos = Player.Position - Position;
        if (playerRelativePos.Length() <= 16) {
            direction = Vector2.Zero;
            lastDirection = playerRelativePos.Normalized();
        } else if (playerRelativePos.Length() <= 64 && bounceCountdown == 0) {
            direction = playerRelativePos.Normalized();
        } else if (bounceCountdown == 0) {
            float v = RandomFloat();
            if (v <= 0.01) {
                direction = Vector2.Zero;
            } else if (v <= 0.1) {
                direction = Vector2.Down.Rotated(GetNoise(Position.x, Position.y, noise) * 2 * Mathf.Pi);
            }
        }

        if (bounceCountdown > 0) {
            bounceCountdown -= 1;
        }
    }

    public override void Spawn()
    {
        animationPlaying = true;
        Show();
        Sprite.Play("birth");
    }

    public void Hit(int damage)
    {
        health -= damage;
        if (health > 0) {
            AnimationPlayer.Play("Hit");
        } else {
            direction = Vector2.Zero;
            SetProcess(false);
            animationPlaying = true;
            Sprite.Play("death");
            EmitSignal("Despawn", this);
        }
    }

    public void _on_AnimatedSprite_animation_finished() 
    {
        if (Sprite.Animation == "birth") {
            Sprite.Animation = "down_idle";
        } else if (Sprite.Animation == "death") {
            Hide();
        }
        animationPlaying = false;
    }

    private void AnimateMonster(Vector2 direction)
    {
        if (direction != Vector2.Zero) {
            lastDirection = direction;
            string animation = GetGridDirection(lastDirection) + "_walk";
            Sprite.Play(animation);
        } else {
            string animation = GetGridDirection(lastDirection) + "_idle";
            Sprite.Play(animation);
        }
    }
}
