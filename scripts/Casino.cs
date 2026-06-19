using Godot;
using System;
using System.Collections.Generic;

public partial class Casino : Sprite2D
{	
	public int Money = 0; // how much money we gots
	public List<int> historicalMoney; // Keeps track of money over time
	public int Taxes; // Daily property taxes that drain your resources

	public float Reputation; // 1-10 / 5 star system

	private int maxNumberMachines = 1; // how many machines you can have at once
	private int MaxNumberMachines
	{
		get => maxNumberMachines;
		set
		{
			maxNumberMachines = value;
			if (currentNumberMachines < maxNumberMachines)
				EnableMachineSlots();
		}
	}
	
	private int currentNumberMachines = 0;
	private int CurrentNumberMachines
	{
		get => currentNumberMachines;
		set
		{
			currentNumberMachines = value;
			if (currentNumberMachines < maxNumberMachines)
				EnableMachineSlots();
		}
	}
	
	private Label timeCounter;
	
	private Counter[] counters = new Counter[3];
	
	// debug/testing
	private PackedScene testMachine = (PackedScene)GD.Load("res://scenes/lotto_machine.tscn");

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Clock.Instance.RestartClock();
		Clock.Instance.TickTock += OnSecondPassed;
		timeCounter = GetNode<Label>("TimeElapsed");
		
		PrepCountersAndButtons();
	}
	
	private void PrepCountersAndButtons()
	{
		for (int i = 0; i < counters.Length; i++)
		{
			counters[i] = GetNode<Counter>("Counter" + i);
			for (int j = 0; j < counters[i].Buttons.Length; j++)
			{
				int ii = i;
				int jj = j;
				
				counters[i].Buttons[j].Pressed +=
					() => AddMachine((LottoMachine)testMachine.Instantiate(), ii, jj);
			}
		}
	}
	
	// counter = which counter
	// slot = which slot on counter
	public bool AddMachine(LottoMachine machine, int counter, int slot)
	{		
		GD.Print("attempting to add machine, " + counter + " " + slot);
				
		if (currentNumberMachines >= maxNumberMachines)
		{
			GD.Print("Too many machines in the casino!");
			return false;
		}
		
		bool success = counters[counter].AddMachine(machine, slot);
		machine.PlayGame += OnPlayGameSignal;

		if (!success)
			GD.Print("failed to add machine AAAAAAA");
			
		GD.Print("machine added successfully");
		
		currentNumberMachines++;
		if (currentNumberMachines >= maxNumberMachines)
			DisableMachineSlots();

		return success;
	}
	
	private void DisableMachineSlots()
	{
		foreach (Counter c in counters)
			c.DisableButtons();
	}
	
	private void EnableMachineSlots()
	{
		foreach (Counter c in counters)
			c.EnableButtons();
	}
	
	// this shoulda been private?
	public void OnPlayGameSignal(int costPerPlay)
	{
		Money += costPerPlay;
		GetNode<Label>("Money").Text = string.Format("${0}", Money);
	}
	
	// called every gameplay second passed
	private void OnSecondPassed()
	{
		timeCounter.Text = Clock.Instance.PlayTimeAsString();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
			else if (keyEvent.Keycode == Key.Key1)
				MaxNumberMachines++;
	}
}
