using Godot;
using System;

public partial class CustomerTravelRegion : CollisionShape2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		randomPoint = GetRandomPoint();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	private Vector2 randomPoint;
	
	public override void _Draw()
	{
		DrawCircle(randomPoint - Position, 5, new Color(0, 1, 1));
	}
	
	private int buffer = 25;
	
	public Vector2 GetRandomPoint()
	{
		float width =  ((RectangleShape2D)Shape).Size.X - buffer;
		float height = ((RectangleShape2D)Shape).Size.Y - buffer;
		GD.Print(width + "," + height);
		float x = Position.X + Global.Random.Next(0, (int)width) - (int)width/2;
		float y = Position.Y + Global.Random.Next(0, (int)height) - (int)height/2;
		GD.Print(x + "," + y);
		return new Vector2(x, y);
	}
}
