using Godot;
using System;

public class Mana : ColorRect
{
    public void OnPlayerStatsChanged(Player player) {
        ColorRect bar = GetChild<ColorRect>(0);
        bar.RectSize = new Vector2(72 * player.mana / player.manaMax, bar.RectSize.y);
    }
}
