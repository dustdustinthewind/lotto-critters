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
	private double[] exponentialMovingAveragePayoutTypes = {1/6.0, 1/6.0, 1/6.0, 1/6.0, 1/6.0, 1/6.0};
	private double[] exponentialMovingAveragePayoutAmount = {0,0,0,0,0,0};
		
	public double Evil; // a compliance/event-important variable

	public int CostPerPlay;
	private double exponentialMovingAverageCost = 0;
	
	public ulong TimeToPlay;
	private ulong whenDonePlaying;
	public bool playing = false;
	
	private bool refractoring = false;
	private ulong refractoryPeriod = 888;
	private ulong whenDoneRefractory;

	public int Attractiveness => baseAttractiveness + NewMachineNovelty; // how attractive/addictive this game is compared to others, how much folks wanna play it
	public int baseAttractiveness;
	
	public int NewMachineNovelty
	{
		get => newMachineNovelty;
		set => newMachineNovelty = value < 0 ? 0 : value;
	}
	private int newMachineNovelty; // an attractiveness bonus from a new machine people are more attracted/curious to try it out
	public int noveltyDrain; // how much NewMachineAttractivenessBonus drains per play
	
	public double ChanceHouseWins;
	
	private Button button;

	// this runs first
	public LottoMachine()
	{
		// evilness
		Evil = .1 + Global.Random.NextDouble() * .8; // between 0.1 and .9
		
		// condition
		MaxCondition = Global.Random.Next(150, 250);
		machineCondition = MaxCondition - (Global.Random.NextDouble() < Evil / 1.5 ? Global.Random.Next(25, 100) : 0);
		conditionDrain = Global.Random.NextDouble() < Evil / 1.2 ? Global.Random.Next(3, 6) : Global.Random.Next(1, 4);
		
		// attractiveness
		baseAttractiveness = Global.Random.Next(40, 100); // range 40-100
		NewMachineNovelty = Global.Random.Next(80, 140); // range 80-140
		noveltyDrain = 100 / baseAttractiveness * Global.Random.Next(2, 4);
		// roll evil attractiveness

		// TimeToPlay
		TimeToPlay = (ulong)Global.Random.Next(950, 1000) * (ulong)Global.Random.Next(8, 15); // ~8-15 seconds
		// if debugging
		TimeToPlay /= 10;
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
		// will be a value between 16 and 28
		int attractivenessBonus = baseAttractiveness / 10 + NewMachineNovelty / 10;
		// choose a value between attractiveness bonus and 20
		attractivenessBonus = Global.Random.Next(attractivenessBonus, 20);
		// make it a negative if we roll below half the evil modifier
		attractivenessBonus *= Global.Random.NextDouble() < Evil/1.67 ? -1 : 1;
		// choose a random number between the two parents attractiveness.
		int parentAttractiveness = randomBetweenTwoInts(parent1.baseAttractiveness, parent2.baseAttractiveness);
		// add genetic attractiveness with the bonus
		baseAttractiveness = parentAttractiveness + attractivenessBonus;
		// recalc novelty drain
		noveltyDrain = Math.Max(parent1.baseAttractiveness, parent2.baseAttractiveness) / baseAttractiveness * Global.Random.Next(2, 4);
		
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
	
	private void OnParlayToggled(bool toggle)
	{
		if (toggle)
		{
			button.Pressed -= statCard.ToggleButtons;
			button.Pressed += PutUpForParlay;
			statCard.DisableButtons();
		}
		else
		{
			button.Pressed -= PutUpForParlay;
			button.Pressed += statCard.ToggleButtons;
		}
	}
	
	private void PutUpForParlay()
	{
		
	}
	
	[Signal]
	public delegate void PlayGameEventHandler(int cost);
		
	private Sprite2D machineSprite;	
	public LottoStatCard statCard;
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		birthTime = Clock.Instance.PlayTimeElapsed;
		
		button = GetNode<Button>("Button");
		
		statCard = (LottoStatCard)GetNode<ColorRect>("StatCard");
		statCard.Visible = false;
		button.MouseEntered += () => statCard.Visible = true;
		button.MouseExited += () => statCard.Visible = false;
		button.Pressed += statCard.ToggleButtons;
		Clock.Instance.TickTock += UpdateStatCard;
		
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
		statCard.CostToPlay = (int)exponentialMovingAverageCost;
		
		statCard.Payouts = new Payout(
			(int)exponentialMovingAveragePayoutAmount[1], exponentialMovingAveragePayoutTypes[1],
			(int)exponentialMovingAveragePayoutAmount[2], exponentialMovingAveragePayoutTypes[2],
			(int)exponentialMovingAveragePayoutAmount[3], exponentialMovingAveragePayoutTypes[3],
			(int)exponentialMovingAveragePayoutAmount[4], exponentialMovingAveragePayoutTypes[4],
			(int)exponentialMovingAveragePayoutAmount[5], exponentialMovingAveragePayoutTypes[5]
		);
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
		
		int costThisPlay = Global.Random.Next(0, CostPerPlay < 10 ? 2 : CostPerPlay < 100 ? CostPerPlay / 5 : CostPerPlay / 10);
		costThisPlay *= Global.Random.NextDouble() < 0.5 ? -1 : 1;
		costThisPlay *= (int)(Global.Random.NextDouble() < Evil ? Global.Random.NextDouble() * 4 : 1);
		costThisPlay = CostPerPlay + costThisPlay;
		
		EmitSignal(SignalName.PlayGame, costThisPlay);
		
		MoneyNotification money = (MoneyNotification)moneyNotif.Instantiate();
		money.Money = costThisPlay;
		AddChild(money);
		
		double alpha = 0.25;
		if (exponentialMovingAverageCost == 0)
			exponentialMovingAverageCost = costThisPlay;
		else
			exponentialMovingAverageCost = alpha * (double)costThisPlay + (1.0 - alpha) * exponentialMovingAverageCost;

		
		// start timer
		playing = true;
		whenDonePlaying = Clock.Instance.PlayTimeElapsed + TimeToPlay;
		machineSprite.Modulate = new Color(0.4f, 0.4f, 0.4f);
		
		UpdateStatCard();
	}
	
	private ProgressBar healthBar;
	private ProgressBar playTimeBar;
	
	public void DoAPayout()
	{
		machineSprite.Modulate = new Color(1, 1, 1);
		
		// refractory period
		refractoring = true;
		whenDoneRefractory = Clock.Instance.PlayTimeElapsed + refractoryPeriod;
		
		// figure out payout to customer that just played
		int thisPayout = Payouts.RollRandomPayout(Evil, ref exponentialMovingAveragePayoutTypes, ref exponentialMovingAveragePayoutAmount);
		if (thisPayout != 0)
		{			
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
		
		// drain novelty
		NewMachineNovelty -= noveltyDrain;
		
		UpdateStatCard();
	}
	
	public bool UpgradeCondition()
	{
		if (machineCondition >= MaxCondition) return false;
		
		machineCondition += conditionDrain * Global.Random.Next(6, 20);
		machineCondition = Math.Min(MaxCondition, machineCondition);
		
		return true;
	}
	
	public bool UpgradeAttractiveness()
	{
		baseAttractiveness += 2 * Global.Random.Next(8, 14);
		newMachineNovelty += noveltyDrain * Global.Random.Next(2, 12);
		
		return true;
	}
	
	public bool UpgradePlayTime()
	{
		if (TimeToPlay == 100u) return false;
		
		TimeToPlay = (ulong)((double)TimeToPlay * (0.88 + (Global.Random.NextDouble() * 0.08)));
		TimeToPlay = Math.Max(100u, TimeToPlay);
		
		return true;
	}
	
	public bool UpgradeCost()
	{
		// look how dust can shove everything into one line so fucking impressive!
		// how the fuck am i supposed to read this
		int oldCostPerPlay = CostPerPlay;
		CostPerPlay = (int)((double)CostPerPlay * (1.02 + (Global.Random.NextDouble() * 0.05)));
		CostPerPlay += oldCostPerPlay == CostPerPlay ? 2 : 0;
		return true;
	}
	
	public bool UpgradePayoutChances()
	{
		return false;
	}
	
	public bool UpgradePayoutAmounts()
	{
		return false;
	}
	
	public bool UpgradeEvil()
	{
		if (Evil <= .1) return false;
		
		Evil -= Global.Random.NextDouble() * 0.07;
		Evil = Math.Max(Evil, .1);
		
		return true;
	}
	
	public bool UpgradeChaos()
	{
		if (Evil >= .9) return false;
		
		Evil += Global.Random.NextDouble() * 0.14;
		Evil = Math.Min(Evil, .92);
		
		return true;
	}
}

public enum PayoutType
{
	HOUSE_WON = 0,
	TINY = 1,
	SMALL = 2,
	MEDIUM = 3,
	LARGE = 4,
	JACKPOT = 5
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

	public int RollRandomPayout(double evil, ref double[] pema, ref double[] aema)
	{
		int payoutToReturn = 0;
		PayoutType outcome = PayoutType.HOUSE_WON;
		
		double roll = Global.Random.NextDouble();

		// send signal based on winnings?
		if (roll < jackpotChance)
		{
			outcome = PayoutType.JACKPOT;
			payoutToReturn = Jackpot;
		}
		else if (roll < jackpotChance + largeChance)
		{
			outcome = PayoutType.LARGE;
			payoutToReturn = LargePayout;
		}
		else if (roll < jackpotChance + largeChance + mediumChance)
		{
			outcome = PayoutType.MEDIUM;
			payoutToReturn = MediumPayout;
		}
		else if (roll < jackpotChance + largeChance + mediumChance + smallChance)
		{
			outcome = PayoutType.SMALL;
			payoutToReturn = SmallPayout;
		}
		else if (roll < jackpotChance + largeChance + mediumChance + smallChance + tinyChance)
		{
			outcome = PayoutType.TINY;
			payoutToReturn = TinyPayout;
		}
		
		double alpha = 0.03;
		
		for (int i = 0; i < pema.Length; i++)
		{
			double wasOutcome = i == (int)outcome ? 1 : 0;
			pema[i] = alpha * wasOutcome + (1.0 - alpha) * pema[i];
		}
		
		if (outcome == PayoutType.HOUSE_WON) return payoutToReturn;
		
		int payoutModifier = payoutToReturn / Global.Random.Next(6, 12);
		payoutToReturn += payoutToReturn / Global.Random.NextDouble() < evil ? payoutModifier : -payoutModifier;
		payoutToReturn = Math.Max(1, payoutToReturn);
		
		alpha = 0.25;
		if (aema[(int)outcome] == 0)
			aema[(int)outcome] = payoutToReturn;
		else
			aema[(int)outcome] = alpha * (double)payoutToReturn + (1.0 - alpha) * aema[(int)outcome];
		
		return payoutToReturn;
	}
}
