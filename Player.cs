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
	public int manaRegeneration = 2;

	[Export]
	public float attackCooldown = 100f;

	private float nextAttackTime = 0f;
	private int attackDamage = 30;
	private Vector2 lastDirection = new Vector2(0, 1);
	private bool attackPlaying = false;
	private bool dragEnabled = false;
	private AnimatedSprite _sprite;
	private RayCast2D _raycast;

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
			movement = 0.3f * movement;
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

		if (@event.IsActionPressed("attack")) {
			var now = OS.GetTicksMsec();
			if (now >= nextAttackTime) {
				var target = RayCast.GetCollider() as Node;
				if (target != null && target.Name.Contains("Skeleton")) {
					(target as Skeleton).Hit(attackDamage);
				}
				attackPlaying = true;
				var animation = GetGridDirection(lastDirection) + "_attack";
				Sprite.Play(animation);
				nextAttackTime = now + attackCooldown;
			}

		} else if (@event.IsActionPressed("fireball")) {
			if (mana >= 25) {
				mana -= 25;
				EmitSignal("PlayerStatsChanged", this);
				attackPlaying = true;
				var animation = GetGridDirection(lastDirection) + "_fireball";
				Sprite.Play(animation);
			}
		}
	}

	public void OnWorldCreated() 
	{
		int x = 15;
		int y = 15;
		for(; x < 60; x++) {
			for(; y < 60; y++) {
				if (TerrainLookup.TryGetValue(new Vector2(x, y), out TerrainType val)) {
					if (val == TerrainType.GRASS) {
						GlobalPosition = new Vector2(x * 32, y * 32);
						return;
					}
				}
			}
		}
	}

	private void _on_Sprite_animation_finished() 
	{
		attackPlaying = false;
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
