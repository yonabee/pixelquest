using Godot;
using System;

public class GUI : CanvasLayer
{
    private Label _skellies;

    private Label Skellies {
		get { 
			if (_skellies == null) {
				_skellies = GetNode<Label>("./Skellies"); 
			}
			return _skellies; 
		}
	}

    public void OnSpawnCountUpdated(string entityName, int count)
    {
        if (entityName == "Skeleton") 
        {
            Skellies.Text = "Skellies " + count.ToString();
        }
    }
}
