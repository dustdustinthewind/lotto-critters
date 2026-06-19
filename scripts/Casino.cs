using Godot;
using System;
using System.Collections.Generic;

public partial class Casino : Sprite2D
{
	public static Casino Instance { get; private set; }
	
	public int Money = 0; // how much money we gots
	public List<int> historicalMoney; // Keeps track of money over time
	public int Taxes; // Daily property taxes that drain your resources

	public float Reputation; // 1-10 / 5 star system

	public int MaxNumberMachines; // how many machines you can have at once

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Clock.Instance.RestartClock();
		Instance = this;
		PackedScene testMachine = (PackedScene)GD.Load("res://scenes/lotto_machine.tscn");
		GetNode<Counter>("Counter").AddMachine((LottoMachine)testMachine.Instantiate());
	}
	
	// this shoulda been private?
	public void OnPlayGameSignal(int costPerPlay)
	{
		Money += costPerPlay;
		GetNode<Label>("Money").Text = string.Format("${0}", Money);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<Label>("TimeElapsed").Text = Clock.Instance.PlayTimeAsString();
	}
	
	public override void _Input(InputEvent @event)
	{
		// pause and unpause the clock wow
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			if (keyEvent.Keycode == Key.Space)
				if (Clock.Instance.IsPaused)
					Clock.Instance.ResumeClock();
				else
					Clock.Instance.PauseClock();
	}
}
