using Godot;
using static GameUtils;
using System;

public class Player : KinematicBody2D
{
	[Export]
	public int speed = 75;

	[Export]
	public float health = 100;

	[Export]
	public int healthMax = 100;

	[Export]
	public int healthRegeneration = 1;

	[Export]
	public float mana = 100;

	[Export]
	public int manaMax = 100;

	[Export]
	public int manaRegeneration = 4;

	[Export]
	public int attackDamage = 30;

	[Export]
	public int fireballDamage = 50;

	private float nextFireballTime = 0f;
	private float fireballCooldown = 100f;
	private float nextAttackTime = 0f;
	public float attackCooldown = 100f;
	private int fireballCost = 5;
	
	private Vector2 lastDirection = new Vector2(0, 1);
	public bool attackPlaying = false;
	private bool dragEnabled = false;
	private AnimatedSprite _sprite;
	private RayCast2D _raycast;
	private bool gameOver = false;

	private AnimationPlayer _animationPlayer;
    private AnimationPlayer AnimationPlayer {
		get { 
			if (_animationPlayer == null) {
				_animationPlayer = GetNode<AnimationPlayer>("./AnimationPlayer");
			}
			return _animationPlayer;
		}
	}

	private AnimatedSprite Sprite {
		get { 
			if (_sprite == null) {
				_sprite = GetNode<AnimatedSprite>("./Sprite");
			}
			return _sprite;
		}
	}

	private RayCast2D RayCast {
		get { 
			if (_raycast == null) {
				_raycast = GetNode<RayCast2D>("./RayCast2D"); 
			}
			return _raycast; 
		}
	}

	private BulletSpawner _bullets;
	private BulletSpawner Bullets {
		get {
			if (_bullets == null) {
				_bullets = GetNode<BulletSpawner>("./BulletSpawner");
			}
			return _bullets;
		}
	}

	[Signal]
	public delegate void PlayerStatsChanged(Player player);

	public override void _Ready()
	{
		this.Connect("PlayerStatsChanged", GetNode<ColorRect>("../GUI/Health"), "OnPlayerStatsChanged");
		this.Connect("PlayerStatsChanged", GetNode<ColorRect>("../GUI/Mana"), "OnPlayerStatsChanged");
		EmitSignal("PlayerStatsChanged", this);
	}

	public override void _Process(float delta)
	{
		float newMana = Mathf.Min(mana + manaRegeneration * delta, manaMax);
		if (newMana != mana) {
			mana = newMana;
			EmitSignal("PlayerStatsChanged", this);
		}
		float newHealth = Mathf.Min(health + healthRegeneration * delta, healthMax);
		if (newHealth != health) {
			health = newHealth;
			EmitSignal("PlayerStatsChanged", this);
		}
		if (!Sprite.IsPlaying()) {
			attackPlaying = false;
		}
	}

	public override void _PhysicsProcess(float delta)
	{
		var direction = new Vector2();
		direction.x = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
		direction.y = Input.GetActionStrength("ui_down") - Input.GetActionStrength("ui_up");

		if (Mathf.Abs(direction.x) == 1 && Mathf.Abs(direction.y) == 1) {
			direction = direction.Normalized();
		}

		var movement = speed * speed * direction * delta;
		if (dragEnabled) {
			var newPos = GetGlobalMousePosition();
			movement = newPos - Position;
			if (movement.Length() > speed * speed * delta) {
				movement = speed * speed * delta * movement.Normalized();
			}
		}
		if (attackPlaying) {
			movement = 0.8f * movement;
		}
		MoveAndSlide(movement);
		if (!attackPlaying) {
			AnimatePlayer(movement.Normalized());
		}

		if (direction != Vector2.Zero) {
			RayCast.CastTo = direction.Normalized() * 16;
		}
	}

	public override void _InputEvent(Godot.Object viewport, InputEvent @event, int shapeIdx) 
	{
		if (@event is InputEventMouseButton) {
			var mouseEvent = @event as InputEventMouseButton;
			if (mouseEvent.ButtonIndex == (int)ButtonList.Left) {
				dragEnabled = true;
			}
		}
	}

	public override void _Input(InputEvent @event) 
	{
		if (@event is InputEventMouseButton) {
			var mouseEvent = @event as InputEventMouseButton;
			if (mouseEvent.ButtonIndex == (int)ButtonList.Left) {
				dragEnabled = false;
			}
		}

		var now = OS.GetTicksMsec();

		if (@event.IsActionPressed("attack")) {
			
			if (now >= nextAttackTime) {
				attackPlaying = true;
				var animation = GetGridDirection(lastDirection) + "_attack";
				Sprite.Animation = animation; 
				Sprite.Frame = 0;
				Sprite.Play();
				nextAttackTime = now + attackCooldown;
			}

		} else if (@event.IsActionPressed("fireball")) {
			if (mana >= fireballCost && now >= nextFireballTime) {
				mana -= fireballCost;
				EmitSignal("PlayerStatsChanged", this);
				attackPlaying = true;
				var animation = GetGridDirection(lastDirection) + "_fireball";
				Sprite.Animation = animation; 
				Sprite.Frame = 0;
				Sprite.Play();
				nextFireballTime = now + fireballCooldown;
			}
		} 
	}

	public void OnWorldCreated() 
	{
		for(int x = 0; x < 64; x++) {
			for(int y = 0; y < 64; y++) {
				Vector2 loc = new Vector2(x,y);
				if (TerrainLookup[loc] == TerrainType.GRASS) {
					GlobalPosition = new Vector2(x * 32, y * 32);
					return;
				}
			}
		}
	}

	public void Hit(int damage) 
	{
		if (gameOver) {
			return;
		}
		health -= damage;
		EmitSignal("PlayerStatsChanged", this);
		if (health <= 0) {
			SetProcess(false);
			gameOver = true;
			AnimationPlayer.Play("GameOver");
		} else {
			AnimationPlayer.Play("Hit");
		}
	}

	private void _on_Sprite_animation_finished() 
	{
		attackPlaying = false;
		if (Sprite.Animation.EndsWith("fireball")) {
			Vector2 pos = GlobalPosition + lastDirection.Normalized() * 4;
			Fireball fireball = Bullets.AwakeEntity(pos) as Fireball;
			fireball.attackDamage = fireballDamage;
			fireball.direction = lastDirection.Normalized();
		}
	}

	private void AnimatePlayer(Vector2 direction)
	{
		var animation = "down_idle";
		if (direction != Vector2.Zero) {
			lastDirection = 0.5f * lastDirection + 0.5f * direction;
			animation = GetGridDirection(lastDirection) + "_walk";
			Sprite.Frames.SetAnimationSpeed(animation, 4 + 12 * direction.Length());
		} else {
			animation = GetGridDirection(lastDirection) + "_idle";
		}
		Sprite.Play(animation);
	} 
}
