using Godot;
using System;

public partial class LottoMachine : Node2D
{
	public int machineCondition; // Hunger and Wear and Tear and Mood that might affect the working of a machine.

	public ulong CurrentAge => Clock.Instance.PlayTimeElapsed - birthTime;
	private ulong birthTime;

	public Payout Payouts;
	
	public double Evil; // a compliance/event-important variable

	public int CostPerPlay;
	
	public ulong TimeToPlay;
	private ulong whenDonePlaying;
	public bool playing = false;

	public int Attractiveness => attractiveness + NewMachineNovelty; // how attractive/addictive this game is compared to others, how much folks wanna play it
	private int attractiveness;
	
	public int NewMachineNovelty
	{
		get => newMachineNovelty;
		set => newMachineNovelty = value < 0 ? 0 : value;
	}
	private int newMachineNovelty; // an attractiveness bonus from a new machine people are more attracted/curious to try it out
	public int noveltyDrain; // how much NewMachineAttractivenessBonus drains per play
	
	public double ChanceHouseWins;
	
	//debug/testing
	private Button button;

	// this runs first
	public LottoMachine()
	{
		birthTime = Clock.Instance.PlayTimeElapsed;

		// evilness
		Evil = Global.Random.NextDouble(); // between 0 and 1
		
		// attractiveness
		attractiveness = Global.Random.Next(40, 100); // range 40-100
		NewMachineNovelty = Global.Random.Next(20, 40); // range 20-40
		noveltyDrain = 100 / attractiveness * Global.Random.Next(2, 4);
		// roll evil attractiveness

		// TimeToPlay
		TimeToPlay = 100u * (ulong)Global.Random.Next(10, 30); // 10-30 seconds  NOTE I SET IT TO 1/10th FOR SANITY DURING TESTING FORGIVE MEEEEEEEE
		// roll evil time

		// CostPerPlay
		double lowMedOrHigh = Global.Random.NextDouble();
		CostPerPlay =
			lowMedOrHigh < 0.6 ? Global.Random.Next(5,10)    :
			lowMedOrHigh < 0.9 ? Global.Random.Next(25, 50)  :
								 Global.Random.Next(100, 500);
		// roll evil cost

		// Payout generation
		SetRandomPayouts();
	}

	// if constructor chaining this runs second
	public LottoMachine(LottoMachine parent1, LottoMachine parent2) : this()
	{
		// genetic-swapping here
		// genes are decided from range of parents
		// genetic bonuses in most cases
		
		// use previously generated attractiveness to determine our attractiveness bonus
		// will be a value between 6 and 14
		int attractivenessBonus = attractiveness / 10 + NewMachineNovelty / 10;
		// choose a value between attractiveness bonus and 20
		attractivenessBonus = Global.Random.Next(attractivenessBonus, 20);
		// make it a negative if we roll below half the evil modifier
		attractivenessBonus *= Global.Random.NextDouble() < Evil/1.67 ? -1 : 1;
		// choose a random number between the two parents attractiveness.
		int parentAttractiveness = Global.Random.Next(Math.Min(parent1.attractiveness, parent2.attractiveness), Math.Max(parent1.attractiveness, parent2.attractiveness));
		// add genetic attractiveness with the bonus
		attractiveness = parentAttractiveness + attractivenessBonus;
		
		// use perviously generated TimeToPlay to determine our bonus to playtime
		// (less time to play is preferrable)
		// will be a value between 1 and 5
		ulong playTimeBonus = TimeToPlay / 6000u;
		// choose a value between playTimeBonus and playTimeBonus * 1.6
		// will be value between 1 and 8 seconds
		playTimeBonus = (ulong)Global.Random.Next((int)playTimeBonus, (int)(playTimeBonus * 1.6));
		// choose a time to play between the two parents time to plays
		ulong parentPlayTime = (ulong)Global.Random.Next(Math.Min((int)parent1.TimeToPlay, (int)parent2.TimeToPlay), Math.Max((int)parent1.TimeToPlay, (int)parent2.TimeToPlay));
		// give new machine a time to play between 100 ms and parentPlayTime+playTimeBonus, depending on whats higher
		// make it negative so we have lower time to play, unless evil roll makes it positive
		TimeToPlay = Global.Random.NextDouble() < Evil*1.33 ?
			Math.Max(100u, parentPlayTime + playTimeBonus)  :
			Math.Max(100u, parentPlayTime - playTimeBonus)  ;
		
		// use previously generated CostPerPlay to determine our bonus to cost
		int costBonus = CostPerPlay / (Global.Random.Next(2, 10));
		// evil modifier to make it negative instead of positive
		costBonus *= Global.Random.NextDouble() < Evil ? -1 : 1;
		// choose a random number between the two parents cost to play
		int parentCostToPlay = Global.Random.Next(Math.Min(parent1.CostPerPlay, parent2.CostPerPlay), Math.Max(parent1.CostPerPlay, parent2.CostPerPlay));
		// add genetic cost to play with the bonus
		CostPerPlay = parentCostToPlay + costBonus;
		
		// Todo: evil shit
		SetRandomPayouts(
			randomBetweenTwoDoubles(parent1.ChanceHouseWins, parent2.ChanceHouseWins),
			randomBetweenTwoDoubles(parent1.Payouts.tinyChance, parent2.Payouts.tinyChance),
			randomBetweenTwoDoubles(parent1.Payouts.smallChance, parent2.Payouts.smallChance),
			randomBetweenTwoDoubles(parent1.Payouts.mediumChance, parent2.Payouts.mediumChance),
			randomBetweenTwoDoubles(parent1.Payouts.largeChance, parent2.Payouts.largeChance),
			randomBetweenTwoDoubles(parent1.Payouts.jackpotChance, parent2.Payouts.jackpotChance)
		);
	}
	
	private double randomBetweenTwoDoubles(double val1, double val2)
	{
		double min = Math.Min(val1, val2);
		double max = Math.Min(val1, val2) - min;
		return min + (Global.Random.NextDouble() * max);
	}

	public void SetRandomPayouts()
	{
		// house chance of winning is 50% to 80%
		// upgradeable to 90%
		SetRandomPayouts(randomBetweenTwoDoubles(0.5, 0.8));
	}

	public void SetRandomPayouts(
		double chanceHouseWins,
		// average chances of certain payouts if house loses
		double tinyChance = 0.5, // 1/2
		double smallChance = 0.2, // 1/5
		double mediumChance = 0.15, // 3/20
		double largeChance =  0.01, // 1/10
		double jackpotChance = 100 // 0.05 // 1/20
	)
	{
		ChanceHouseWins = chanceHouseWins;
		double chancePlayerWins = 1.0-chanceHouseWins;
		
		// TODO: evil modifications
		Payouts = new Payout(
			// tiny payout = CostPerPlay / (3-10)
			(int)Math.Ceiling((double)CostPerPlay / Global.Random.Next(3, 10)),
				chancePlayerWins * tinyChance,
			// small payout = CostPerPlay / (1.0-2.0)
			(int)((double)CostPerPlay / (1 + Global.Random.NextDouble())),
				chancePlayerWins * smallChance,
			// medium payout = CostPerPlay * (1.1-1.5)
			(int)((double)CostPerPlay * (1.1 + Global.Random.NextDouble() * 0.4)),
				chancePlayerWins * mediumChance,
			// large payout = CostPerPlay * (2-5)
			CostPerPlay * Global.Random.Next(2, 5),
				chancePlayerWins * largeChance,
			// jackpot payout = CostPerPlay * (10-40)
			CostPerPlay * Global.Random.Next(10, 40),
				chancePlayerWins * jackpotChance
		);
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
		//button.Pressed += PlayLottoGame();
		
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
		money.Money = CostPerPlay;
		AddChild(money);
		
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
		int payout = Payouts.RollRandomPayout();
		if (payout != 0)
		{
			MoneyNotification loss = (MoneyNotification)moneyNotif.Instantiate();
			loss.Money = -payout;
			AddChild(loss);
			EmitSignal(SignalName.PlayGame, -payout);
		}
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

	public int RollRandomPayout()
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
