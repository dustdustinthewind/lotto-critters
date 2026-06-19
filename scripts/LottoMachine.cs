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

	public float Attractiveness; // how attractive/addictive this game is compared to others, how much folks wanna play it
	public float NewMachineAttractivenessBonus; // a "novelty" from a new machine people are more attracted/curious to try it out
	public float noveltyDrain;

	public LottoMachine(LottoMachine parent1, LottoMachine parent2) : this()
	{
		// genetic-swapping here
		// genes are decided from range of parents
		// genetic bonuses in most cases
	}

	public LottoMachine()
	{
		birthTime = Clock.Instance.PlayTimeElapsed;
		// random stats here
	}
	
	[Signal]
	public delegate void PlayGameEventHandler(int cost);
		
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// this should be handled by casino not global shenanigans?
		PlayGame += Casino.Instance.OnPlayGameSignal;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	// start playing, put money in towards casino total money
	public void PlayLottoGame()
	{
		EmitSignal(SignalName.PlayGame, CostPerPlay);
		// start timer
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			switch (keyEvent.Keycode)
			{
				case Key.Key1:
					PlayLottoGame();
					break;
			}
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

	public int RandomPayout(int bet)
	{
		// make this globally accessible
		Random rndObj = new Random();
		double roll = rndObj.NextDouble();

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
