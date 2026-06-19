using Godot;
using System;

public partial class LottoMachine : Node2D
{
	public int machineCondition; // Hunger and Wear and Tear and Mood that might affect the working of a machine.

	public ulong CurrentAge => Clock.Instance.PlayTimeElapsed - birthTime;
	private ulong birthTime;

	public Payout Payouts;
	
	public float Evil; // kinda of a compliance/event-important variable

	public int CostPerPlay = 69; // controllable by player
	
	public ulong TimeToPlay;
	private ulong whenDonePlaying;
	private bool playing = false;

	public int Attractiveness => attractiveness + NewMachineNovelty; // how attractive/addictive this game is compared to others, how much folks wanna play it
	private int attractiveness;
	
	public int NewMachineNovelty
	{
		get => newMachineNovelty;
		set => newMachineNovelty = value < 0 ? 0 : value;
	}
	private int newMachineNovelty; // an attractiveness bonus from a new machine people are more attracted/curious to try it out
	public int noveltyDrain; // how much NewMachineAttractivenessBonus drains per play
	
	//debug/testing
	private Button button;

	// this runs first
	public LottoMachine()
	{
		birthTime = Clock.Instance.PlayTimeElapsed;
		
		// attractiveness
		attractiveness = Global.Random.Next(40, 100); // range 40-100
		NewMachineNovelty = Global.Random.Next(20, 40); // range 20-40
		noveltyDrain = 100 / attractiveness * Global.Random.Next(1, 4);
		
		// TimeToPlay
		TimeToPlay = 1000u * (ulong)Global.Random.Next(3, 5); // 10-30 seconds
	}

	// if constructor chaining this runs second
	public LottoMachine(LottoMachine parent1, LottoMachine parent2) : this()
	{
		// genetic-swapping here
		// genes are decided from range of parents
		// genetic bonuses in most cases
	}
	
	[Signal]
	public delegate void PlayGameEventHandler(int cost);
		
	private Sprite2D machineSprite;	
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		button = GetNode<Button>("Button");
		
		// this should be handled by casino not global shenanigans?
		//PlayGame += Casino.Instance.OnPlayGameSignal;
		
		// get button signal
		button.Pressed += PlayLottoGame;
		
		machineSprite = GetNode<Sprite2D>("Sprite2D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (playing && Clock.Instance.PlayTimeElapsed > whenDonePlaying)
		{
			playing = false;
			DoAPayout();
		}
	}
	
	private PackedScene moneyNotif = (PackedScene)GD.Load("res://scenes/money_notification.tscn");	
	
	// start playing, put money in towards casino total money
	public void PlayLottoGame()
	{
		button.Disabled = true;
		
		EmitSignal(SignalName.PlayGame, CostPerPlay);
		
		MoneyNotification money = (MoneyNotification)moneyNotif.Instantiate();
		AddChild(money);
		money.Money = CostPerPlay;
		
		// start timer
		playing = true;
		whenDonePlaying = Clock.Instance.PlayTimeElapsed + TimeToPlay;
		machineSprite.Modulate = new Color(0.4f, 0.4f, 0.4f);
	}
	
	public void DoAPayout()
	{
		button.Disabled = false;
		machineSprite.Modulate = new Color(1, 1, 1);
		// figure out payout to customer that just played
	}
}

public struct Payout
{
	public int TinyPayout;
	public double tinyChance;
	
	public int SmallPayout;
	public double smallChance;
	
	public int MediumPayout;
	public double mediumChance;
	
	public int LargePayout;
	public double largeChance;
	
	public int Jackpot;
	public double jackpotChance;
	
	public Payout
	(
		int tiny, double tinyPercent,
		int small, double smallPercent,
		int medium, double mediumPercent,
		int large, double largePercent,
		int jackpot, double jackpotPercent
	)
	{
		TinyPayout = tiny; tinyChance = tinyPercent;
		SmallPayout = small; smallChance = smallPercent;
		MediumPayout = medium; mediumChance = mediumPercent;
		LargePayout = large; largeChance = largePercent;
		Jackpot = jackpot; jackpotChance = jackpotPercent;
	}

	public int RandomPayout(int bet)
	{
		double roll = Global.Random.NextDouble();

		// send signal based on winnings?
		if (roll < jackpotChance)
			return Jackpot;
		else if (roll < jackpotChance + largeChance)
			return LargePayout;
		else if (roll < jackpotChance + largeChance + mediumChance)
			return MediumPayout;
		else if (roll < jackpotChance + largeChance + mediumChance + smallChance)
			return SmallPayout;
		else if (roll < jackpotChance + largeChance + mediumChance + smallChance + tinyChance)
			return TinyPayout;
		else
			return 0;
	}
}
