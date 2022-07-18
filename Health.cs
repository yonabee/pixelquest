using Godot;
using System;

public class Health : ColorRect
{
    public void OnPlayerStatsChanged(Player player) {
        ColorRect bar = GetChild<ColorRect>(0);
        bar.RectSize = new Vector2(72 * player.health / player.healthMax, bar.RectSize.y);
    }
}
