using Godot;
using System;
using System.Collections.Generic;

public partial class ResourcePanel : HBoxContainer
{
	public Label GoldLabel;
	public Label WoodLabel;
	public void CreateResourcePanel() {
		foreach(Label label in GetChildren()) {
			label.QueueFree();
		}

		GoldLabel = new();
		AddChild(GoldLabel);

		WoodLabel = new();
		AddChild(WoodLabel);
	}

	public override void _Process(double delta)
	{
		if(GetChildCount() == 0) {
			CreateResourcePanel();
		}
		
		GoldLabel.Text = "Gold: " + World.Instance.Gold;
		WoodLabel.Text = "Wood: " + World.Instance.Wood;
	}
}
