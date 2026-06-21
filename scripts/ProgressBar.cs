using Godot;
using System;

public partial class ProgressBar : Polygon2D
{
	// assumes rectangle bar that drains from right to left
	
	public float Max;
	public float Current;
	public float Min = 0;
	
	public void SetUpBar(float current, float maxvalue, float minvalue = 0)
	{
		Max = maxvalue;
		Current = current;
		Min = minvalue;
		ChangeValue(Current);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ChangeValue(Current);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void ChangeValue(float val)
	{
		Current = val;
		
		SetScale(new Vector2(Math.Max(0, Current / Max), 1));
	}
}
