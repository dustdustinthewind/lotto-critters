using Godot;
using System;

public partial class LottoStatCard : ColorRect
{
	private Label ageLabel;
	public ulong Age
	{
		set => ageLabel.Text = "Age: " + Clock.Instance.TimeAsString(value/1000);
	}

	private Label attractivenessLabel;
	public int Attractiveness
	{
		set => attractivenessLabel.Text = "Attractiveness: " + value;
	}

	private Label conditionLabel;
	public int Condition
	{
		set => conditionLabel.Text = "Condtion: " + value;
	}

	private Label evilLabel;
	public double Evil
	{
		set => evilLabel.Text = String.Format("Evil: {0:P2}", value);
	}

	private Label playTimeLabel;
	public ulong TimeToPlay
	{
		set => playTimeLabel.Text = String.Format("Time To Play: {0:F1} seconds", (double)value / 1000);
	}
	
	private Label costLabel;
	public int CostToPlay
	{
		set => costLabel.Text = "Cost To Play: $" + value;
	}
	
	private Label tinyChance; private Label tinyPayout;
	private Label smallChance; private Label smallPayout;
	private Label mediumChance; private Label mediumPayout;
	private Label largeChance; private Label largePayout;
	private Label jackpotChance; private Label jackpotPayout;
	public Payout Payouts
	{
		set
		{
			tinyChance.Text = String.Format("{0:P2}", value.tinyChance);
			tinyPayout.Text = "$" + value.TinyPayout;
			smallChance.Text = String.Format("{0:P2}", value.smallChance);
			smallPayout.Text = "$" + value.SmallPayout;
			mediumChance.Text = String.Format("{0:P2}", value.mediumChance);
			mediumPayout.Text = "$" + value.MediumPayout;
			largeChance.Text = String.Format("{0:P2}", value.largeChance);
			largePayout.Text = "$" + value.LargePayout;
			jackpotChance.Text = String.Format("{0:P2}", value.jackpotChance);
			jackpotPayout.Text = "$" + value.Jackpot;
			
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		string statLocation = "ColorRect/HBoxContainer/Stats/";

		ageLabel = GetNode<Label>(statLocation + "Age");
		attractivenessLabel = GetNode<Label>(statLocation + "Attractiveness");
		conditionLabel = GetNode<Label>(statLocation + "Condition");
		evilLabel = GetNode<Label>(statLocation + "Evil");
		playTimeLabel = GetNode<Label>(statLocation + "TimeToPlay");
		costLabel = GetNode<Label>(statLocation + "CostToPlay");
		
		string payoutLocation = "ColorRect/HBoxContainer/Payout/";
		tinyChance = GetNode<Label>(payoutLocation + "Tiny/Chance");
		tinyPayout = GetNode<Label>(payoutLocation + "Tiny/Payout");
		smallChance = GetNode<Label>(payoutLocation + "Small/Chance");
		smallPayout = GetNode<Label>(payoutLocation + "Small/Payout");
		mediumChance = GetNode<Label>(payoutLocation + "Medium/Chance");
		mediumPayout = GetNode<Label>(payoutLocation + "Medium/Payout");
		largeChance = GetNode<Label>(payoutLocation + "Large/Chance");
		largePayout = GetNode<Label>(payoutLocation + "Large/Payout");
		jackpotChance = GetNode<Label>(payoutLocation + "Jackpot/Chance");
		jackpotPayout = GetNode<Label>(payoutLocation + "Jackpot/Payout");
		
		ZIndex = 99;
		
		if (GlobalPosition.Y < 0)
			Position += new Vector2(0, 500);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
