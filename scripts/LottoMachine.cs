using Godot;
using System;

public partial class LottoMachine : Node2D
{
	public int machineCondition; // Hunger and Wear and Tear and Mood that might affect the working of a machine.
	public int MaxCondition;
	public int conditionDrain; // how much per play does a machine usually drop

	public ulong CurrentAge => Clock.Instance.PlayTimeElapsed - birthTime;
	private ulong birthTime;

	public Payout Payouts;
	
	public double Evil; // a compliance/event-important variable

	public int CostPerPlay;
	
	public ulong TimeToPlay;
	private ulong whenDonePlaying;
	public bool playing = false;
	
	private bool refractoring = false;
	private ulong refractoryPeriod = 888;
	private ulong whenDoneRefractory;

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
		// evilness
		Evil = Global.Random.NextDouble(); // between 0 and 1
		
		// condition
		MaxCondition = Global.Random.Next(150, 250);
		machineCondition = MaxCondition - (Global.Random.NextDouble() < Evil / 1.5 ? Global.Random.Next(25, 100) : 0);
		conditionDrain = Global.Random.NextDouble() < Evil / 1.2 ? Global.Random.Next(3, 6) : Global.Random.Next(1, 4);
		
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
		
		// use previously generated condition values to determine our condition bonus
		int conditionBonus = MaxCondition / 10;
		double prevConditionRatio = machineCondition / MaxCondition;
		// evil
		conditionBonus *= Global.Random.NextDouble() < Evil/1.67 ? -1 : 1;
		// parent max condition
		int parentCondition = randomBetweenTwoInts(parent1.MaxCondition, parent2.MaxCondition);
		// new max condition
		MaxCondition = parentCondition + conditionBonus;
		// fuck more calcs lets just use the previous ratio to determine how much current
		// condition we have
		machineCondition = (int)(prevConditionRatio * MaxCondition);
		
		// use previously generated attractiveness to determine our attractiveness bonus
		// will be a value between 6 and 14
		int attractivenessBonus = attractiveness / 10 + NewMachineNovelty / 10;
		// choose a value between attractiveness bonus and 20
		attractivenessBonus = Global.Random.Next(attractivenessBonus, 20);
		// make it a negative if we roll below half the evil modifier
		attractivenessBonus *= Global.Random.NextDouble() < Evil/1.67 ? -1 : 1;
		// choose a random number between the two parents attractiveness.
		int parentAttractiveness = randomBetweenTwoInts(parent1.attractiveness, parent2.attractiveness);
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
		ulong parentPlayTime = (ulong)randomBetweenTwoInts((int)parent1.TimeToPlay, (int)parent2.TimeToPlay);
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
		int parentCostToPlay = randomBetweenTwoInts(parent1.CostPerPlay, parent2.CostPerPlay);
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
	
	private int randomBetweenTwoInts(int val1, int val2)
	{
		int min = Math.Min(val1, val2);
		int max = Math.Max(val1, val2);
		return Global.Random.Next(min, max);
	}
	
	private double randomBetweenTwoDoubles(double val1, double val2)
	{
		double min = Math.Min(val1, val2);
		double max = Math.Max(val1, val2) - min;
		return min + (Global.Random.NextDouble() * max);
	}

	public void SetRandomPayouts()
	{
		// house chance of winning is 40% to 80%
		// upgradeable to 90%
		// evil?
		SetRandomPayouts(randomBetweenTwoDoubles(0.4, 0.8));
	}

	public void SetRandomPayouts(
		double chanceHouseWins,
		// average chances of certain payouts if house loses
		double tinyChance = 0.5, // 1/2
		double smallChance = 0.2, // 1/5
		double mediumChance = 0.15, // 3/20
		double largeChance =  0.1, // 1/10
		double jackpotChance = 0.05 // 0.05 // 1/20
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
	private LottoStatCard statCard;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		birthTime = Clock.Instance.PlayTimeElapsed;
		
		button = GetNode<Button>("Button");
		
		statCard = (LottoStatCard)GetNode<ColorRect>("StatCard");
		statCard.Visible = false;
		button.MouseEntered += () => statCard.Visible = true;
		button.MouseExited += () => statCard.Visible = false;
		Clock.Instance.TickTock += UpdateStatCard;
		// this should be handled by casino not global shenanigans?
		//PlayGame += Casino.Instance.OnPlayGameSignal;

		// get button signal
		//button.Pressed += PlayLottoGame();
		
		machineSprite = GetNode<Sprite2D>("Sprite2D");
		
		healthBar = (ProgressBar)GetNode<Polygon2D>("HealthBar");
		healthBar.SetUpBar(machineCondition, MaxCondition);
		
		playTimeBar = (ProgressBar)GetNode<Polygon2D>("PlayTimeBar");
		playTimeBar.SetUpBar(0, TimeToPlay);
		
		UpdateStatCard();
	}
	
	private void UpdateStatCard()
	{
		statCard.Age = CurrentAge;
		statCard.Attractiveness = Attractiveness;
		statCard.Condition = machineCondition;
		statCard.Evil = Evil;
		statCard.TimeToPlay = TimeToPlay;
		statCard.CostToPlay = CostPerPlay;
		
		statCard.Payouts = Payouts;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ulong timeElapsed = Clock.Instance.PlayTimeElapsed;
		if (playing)
		{
			playTimeBar.ChangeValue(whenDonePlaying - timeElapsed);
			
			if (timeElapsed > whenDonePlaying)
			{
				playing = false;
				DoAPayout();
			}
		}
		else if (refractoring && timeElapsed > whenDoneRefractory)
		{
			refractoring = false;
		}
	}
	
	private PackedScene moneyNotif = (PackedScene)GD.Load("res://scenes/money_notification.tscn");	
	
	// start playing, put money in towards casino total money
	public void PlayLottoGame()
	{
		if (playing || refractoring) return;
		
		button.Disabled = true;
		
		int costThisPlay = Global.Random.Next(0, CostPerPlay < 10 ? 2 : CostPerPlay < 100 ? CostPerPlay / 5 : CostPerPlay / 10);
		costThisPlay *= Global.Random.NextDouble() < 0.5 ? -1 : 1;
		costThisPlay *= (int)(Global.Random.NextDouble() < Evil ? Global.Random.NextDouble() * 4 : 1);
		costThisPlay = CostPerPlay + costThisPlay;
		
		EmitSignal(SignalName.PlayGame, costThisPlay);
		
		MoneyNotification money = (MoneyNotification)moneyNotif.Instantiate();
		money.Money = costThisPlay;
		AddChild(money);
		
		// start timer
		playing = true;
		whenDonePlaying = Clock.Instance.PlayTimeElapsed + TimeToPlay;
		machineSprite.Modulate = new Color(0.4f, 0.4f, 0.4f);
	}
	
	private ProgressBar healthBar;
	private ProgressBar playTimeBar;
	
	public void DoAPayout()
	{
		button.Disabled = false;
		machineSprite.Modulate = new Color(1, 1, 1);
		
		// refractory period
		refractoring = true;
		whenDoneRefractory = Clock.Instance.PlayTimeElapsed + refractoryPeriod;
		
		// figure out payout to customer that just played
		int thisPayout = Payouts.RollRandomPayout();
		if (thisPayout != 0)
		{
			int payoutModifier = thisPayout / Global.Random.Next(6, 12);
			thisPayout += thisPayout / Global.Random.NextDouble() < Evil ? payoutModifier : -payoutModifier;
			thisPayout = Math.Max(1, thisPayout);
			
			MoneyNotification loss = (MoneyNotification)moneyNotif.Instantiate();
			loss.Money = -thisPayout;
			AddChild(loss);
			EmitSignal(SignalName.PlayGame, -thisPayout);
		}
		
		// drain condition
		// should be signals? meh
		machineCondition -= conditionDrain;
		healthBar.ChangeValue(machineCondition);
		playTimeBar.ChangeValue(0);
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
