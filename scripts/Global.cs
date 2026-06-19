using Godot;
using System;

public partial class Global : Node
{	
	// make this globally accessible
	public static Random Random = new Random();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
