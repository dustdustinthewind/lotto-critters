using Godot;
using System;

public partial class LottoMachine : Node
{
	public float Health; 
  public float healthDrain; // how fast current health drains due to factors like hunger/wear
	public float maxHealth; // the current max health
	public float maxHealthLossRate; // how fast max health drains due to factors like age
	
	public int CurrentAge;

  public Payout Payouts;
	
  public float Mood; // from 0 (pissed/uncompliant) to 10 (content/compliant)
  public float moodDrain; // Neuroticism, could be negative for positive mood gain
  
  public float Evil; // lol

  public int CostPerPlay; // controllable by player
  public int TimeToPlay;

  public float Attractiveness; // how attractive/addictive this game is compared to others, how much folks wanna play it
  public float NewMachineAttractivenessBonus; // a "novelty" from a new machine people are more attracted/curious to try it out
  public float noveltyDrain;

  public LottoMachine(LottoMachine parent1, LottoMachine parent2)
  {
	// genetic-swapping here
  }
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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
