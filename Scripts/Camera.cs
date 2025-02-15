using Godot;
using System;

public partial class Camera : Camera2D
{
	public Vector2 Start;
	public Vector2 End;
	bool IsDragging = false;
	[Export] float PanDistance = 20f;
	[Export] float PanSpeed = 200f;
	[Signal] public delegate void AreaSelectedEventHandler(Area2D area);
	Panel Box;
	Background Background;

    public override void _Ready()
    {
        Box = GetNode<Panel>("../Panel");
		Background = GetNode<Background>("../Background");
    }

    public override void _Process(double delta)
    {
		Vector2 direction = Vector2.Zero;

		if(GetViewport().GetMousePosition().X > GetViewportRect().Size.X - PanDistance) {
			direction.X = 1;
		}
		if(GetViewport().GetMousePosition().X < 0 + PanDistance) {
			direction.X = -1;
		}
		if(GetViewport().GetMousePosition().Y > GetViewportRect().Size.Y - PanDistance) {
			direction.Y = 1;
		}
		if(GetViewport().GetMousePosition().Y < 0 + PanDistance) {
			direction.Y = -1;
		}

		Position += direction * PanSpeed * (float) delta;
		Background.GlobalPosition += direction * PanSpeed * (float) delta * Background.ParalaxFactor;

		if(Input.IsActionJustPressed("home")) {
			GlobalPosition = Vector2.Zero;
		}

		if(Input.IsActionJustPressed("LeftClick")) {
			Start = GetGlobalMousePosition();
			IsDragging = true;
		}

		if(IsDragging) {
			End = GetGlobalMousePosition();
			DrawArea();
		}

		if(Input.IsActionJustReleased("LeftClick")) {
			End = GetGlobalMousePosition();
			IsDragging = false;
			DrawArea(false);
			EmitSignal("AreaSelected", GetNode<Area2D>("../Panel/Area2D"));
		}
    }

    public void DrawArea(bool s = true) {
		Box.Size = new Vector2(Mathf.Abs(Start.X - End.X), Mathf.Abs(Start.Y - End.Y));
		Vector2 pos = new();
		pos.X = Mathf.Min(Start.X, End.X);
		pos.Y = Mathf.Min(Start.Y, End.Y);
		Box.Position = pos;
		Box.Size *= new Vector2(Convert.ToInt32(s), Convert.ToInt32(s));
		Box.GetNode<Area2D>("Area2D").Scale = Box.Size;
	}

	
}
