using Godot;
using System;

public partial class LottoStatCard : ColorRect
{
	private Label ageLabel;
	public ulong Age
	{
		set => ageLabel.Text = "Age: " + Clock.Instance.TimeAsString(value/1000);
	}

	private Label attractivenessLabel;
	public int Attractiveness
	{
		set => attractivenessLabel.Text = "Attractiveness: " + value;
	}

	private Label conditionLabel;
	public int Condition
	{
		set => conditionLabel.Text = "Condtion: " + value;
	}

	private Label evilLabel;
	public double Evil
	{
		set => evilLabel.Text = String.Format("Evil: {0:P2}", value);
	}

	private Label playTimeLabel;
	public ulong TimeToPlay
	{
		set => playTimeLabel.Text = "Time To Play: " + (double)value / 1000 + " seconds";
	}
	
	private Label costLabel;
	public int CostToPlay
	{
		set => costLabel.Text = "Cost To Play: $" + value;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string statLocation = "ColorRect/HBoxContainer/Stats/";

		ageLabel = GetNode<Label>(statLocation + "Age");
		attractivenessLabel = GetNode<Label>(statLocation + "Attractiveness");
		conditionLabel = GetNode<Label>(statLocation + "Condition");
		evilLabel = GetNode<Label>(statLocation + "Evil");
		playTimeLabel = GetNode<Label>(statLocation + "TimeToPlay");
		costLabel = GetNode<Label>(statLocation + "CostToPlay");
		
		ZIndex = 99;
		
		if (GlobalPosition.Y < 0)
		{
			GD.Print("FUCK");
			Position += new Vector2(0, 300);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
