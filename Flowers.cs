using Godot;
using System;

public class Flowers : Area2D
{
    void _on_Flowers_body_entered(Node body)
    {
        if (body.Name == "Player") {
            GetTree().QueueDelete(this);
        }
    }
}
