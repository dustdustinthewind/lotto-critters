using Godot;
using System;
using System.Collections.Generic;

public partial class Casino : Sprite2D
{	
	public int Money = 0; // how much money we gots
	public List<int> historicalMoney; // Keeps track of money over time
	public int Taxes; // Daily property taxes that drain your resources

	public float Reputation; // 1-10 / 5 star system
	
	[Signal]
	public delegate void CasinoHasResourcesEventHandler();
	[Signal]
	public delegate void CasinoHasNoResourcesEventHandler();
	
	private Label upgrades;
	
	public int UpgradeResources
	{
		get => upgradeResources;
		set
		{
			if (upgradeResources == 0 && value != 0)
				EmitSignal(SignalName.CasinoHasResources);
			else if (upgradeResources != 0 && value == 0)
				EmitSignal(SignalName.CasinoHasNoResources);
			upgrades.Text = "" + value;
			upgradeResources = value;
		}
	}
	private int upgradeResources = 0;

	private int maxNumberMachines = 16; // how many machines you can have at once
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

	private Button parlayButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Clock.Instance.RestartClock();
		Clock.Instance.TickTock += OnSecondPassed;
		timeCounter = GetNode<Label>("TimeElapsed");
		
		moneyLabel = GetNode<Label>("Money");
		upgrades = GetNode<Label>("Upgrades");;
		
		parlayDoor = GetNode<TextureButton>("ParlayDoor");
		parlayButton = GetNode<Button>("ParlayButton");
		
		customer = (Customer)GetNode<Node2D>("Customer");
		PrepCountersAndButtons();
	}
	
	//debug/testing
	private Customer customer;
	
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
					() => AddMachine(ii, jj);
			}
		}
	}

	private TextureButton parlayDoor;
	
	// counter = which counter
	// slot = which slot on counter
	public bool AddMachine(int counter, int slot)
	{
		// debug
		theChild = theChild == null ? (LottoMachine)testMachine.Instantiate() : theChild;
		
		if (theChild == null) return false;
		LottoMachine machine = theChild;
		
		GD.Print("attempting to add machine, " + counter + " " + slot);
				
		if (currentNumberMachines >= maxNumberMachines)
		{
			GD.Print("Too many machines in the casino!");
			return false;
		}
		
		bool success = counters[counter].AddMachine(machine, slot);
		machine.WhenPlaced();
		theChild = null;
		machine.PlayGame += OnPlayGameSignal;
		machine.IWannaParlay += ParlayTime;
		parlayDoor.Toggled += machine.OnParlayToggled;
		
		// i'm sorry for the stupid shitty code but
		// i'm not gonna figure the fuck out of func delegates or
		// whatever the fuck fuck you
		// FUCK C#
		machine.statCard.upgradeCondition.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradeCondition()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradeAttractiveness.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradeAttractiveness()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradePlayTime.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradePlayTime()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradeCost.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradeCost()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradeChances.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradePayoutChances()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradePayouts.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradePayoutAmounts()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradeEvil.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradeEvil()) return;
			UpgradeResources--;
		};
		machine.statCard.upgradeChaos.Pressed += () => 
		{
			if (UpgradeResources <= 0) return;
			if (!machine.UpgradeChaos()) return;
			UpgradeResources--;
		};
		
		CasinoHasNoResources += machine.statCard.OnCasinoHasNoUpgrades;
		CasinoHasResources += machine.statCard.OnCasinoHasUpgrades;

		if (!success)
			GD.Print("failed to add machine AAAAAAA");
			
		GD.Print("machine added successfully");
		
		if (!customer.IsWalking)
			customer.ChooseMachineToWalkTo(counters[counter], counters[counter].Machines[slot]);
		
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
	
	private Label moneyLabel;
	
	private void OnCustomerConsumed()
	{
		GD.Print("customer ated");
		UpgradeResources++;
	}
	
	private void OnPlayGameSignal(int costPerPlay)
	{
		Money += costPerPlay;
		moneyLabel.Text = "$" + Math.Abs(Money);
		if (Money < 0)
		{
			moneyLabel.Text = "-" + moneyLabel.Text;
			moneyLabel.GetLabelSettings().SetFontColor(new Color(176f/255f, 0, 6f/255f));
		}
		else
			moneyLabel.GetLabelSettings().SetFontColor(new Color(0, 176f/255f, 6f/255f));
	}
	
	private LottoMachine parent1;
	private LottoMachine parent2;
	private Vector2 parent1OldPosition;
	private Vector2 parent2OldPosition;
	
	private LottoMachine theChild;
	
	private void ParlayTime(LottoMachine parent)
	{
		if (parent1 == null)
		{
			parent1 = parent;
			parent1OldPosition = parent1.GlobalPosition;
			parent1.GlobalPosition = new Vector2(860, 280); // hard coding *dabs*
			((Sprite2D)GetNode<Sprite2D>("Spotlight2/SpotlightLightShaft")).Visible = true;
			return;
		}
		// if we already selected first parent
		parent2 = parent;
		parent2OldPosition = parent2.GlobalPosition;
		parent2.GlobalPosition = new Vector2(260, 280);
		((Sprite2D)GetNode<Sprite2D>("Spotlight/SpotlightLightShaft")).Visible = true;
		parlayButton.Visible = true;
		parlayButton.Pressed += MakeChild;
	}
	
	// parlay/breed
	private void MakeChild()
	{
		((Sprite2D)GetNode<Sprite2D>("Spotlight2/SpotlightLightShaft")).Visible = false;
		((Sprite2D)GetNode<Sprite2D>("Spotlight/SpotlightLightShaft")).Visible = false;
		theChild = (LottoMachine)testMachine.Instantiate();
		theChild.modifyMachineWithParents(parent1, parent2);
		AddChild(theChild);
		parlayButton.Pressed -= MakeChild;
		parlayButton.Visible = false;
		parent2.GlobalPosition = parent2OldPosition;
		parent1.GlobalPosition = parent1OldPosition;
		parent1 = null;
		parent2 = null;
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
		if (@event is InputEventMouseMotion mouseMoveEvent)
		{			
			if (theChild != null)
			{
				theChild.GlobalPosition = mouseMoveEvent.Position;
				theChild.GlobalPosition += new Vector2(50,0);
			}
		}
	}
}
