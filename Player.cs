using Godot;
using System;

public class Player : KinematicBody2D
{
	[Export]
	private int speed = 75;

	private Vector2 lastDirection = new Vector2(0, 1);
	private bool attackPlaying = false;
	private bool dragEnabled = false;

	private AnimatedSprite Sprite {
		get { return GetChild<AnimatedSprite>(0); }
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
			attackPlaying = true;
			var animation = GetGridDirection(lastDirection) + "_attack";
			Sprite.Play(animation);
		} else if (@event.IsActionPressed("fireball")) {
			attackPlaying = true;
			var animation = GetGridDirection(lastDirection) + "_fireball";
			Sprite.Play(animation);
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

	private string GetGridDirection(Vector2 direction)
	{
		var norm = direction.Normalized();
		if (norm.y >= 0.707) {
			return "down";
		} else if (norm.y <= -0.707) {
			return "up";
		} else if (norm.x <= -0.707) {
			return "left";
		} else if (norm.x >= 0.707) {
			return "right";
		}
		return "down";
	}
}
