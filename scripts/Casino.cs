using Godot;
using System;
using System.Collections.Generic;

public partial class Casino : Node
{
  public int Money; // how much money we gots
  public List<int> historicalMoney; // Keeps track of money over time
  public int Taxes; // Daily property taxes that drain your resources

  public float Reputation; // 1-10 / 5 star system

  private Clock clock = new Clock();

  public int MaxNumberMachines; // how many machines you can have at once

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		clock.RestartClock();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Label>("TimeElapsed").Text = clock.PlayTimeAsString();
	}
	
	public override void _Input(InputEvent @event)
	{
		// pause and unpause the clock wow
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			if (keyEvent.Keycode == Key.Space)
				if (clock.IsPaused)
					clock.ResumeClock();
				else
					clock.PauseClock();
	}
}
