using Godot;
using System;

public partial class Customer : Node2D
{
	public int Mood; // how likely are they to stay at a machine or leave
	public double Stay; // how long until a new Want is generated
	public double stayDelay;
	public int Patience; // when patience runs out they leave
	public int Want; // rng number for modulating mood

	
	private LottoMachine desiredMachine;

	private Button selfButton;
	private Sprite2D sprite;
	private Sprite2D sprite2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Mood = GD.RandRange(1000, 5000);
		Stay = GD.RandRange(3, 8);
		stayDelay = Stay;
		Want = 0;
		Patience = GD.RandRange(1, 8);

		selfButton = GetNode<Button>("Button");
		sprite = GetNode<Sprite2D>("Sprite2D");
		sprite2 = GetNode<Sprite2D>("Sprite2D2");
		// iknow we should just be changing sprites but fuck you,. easier than
		// changing offsets and scales everyt ime


	}
	
	private float speed = 100;
	
	[Signal]
	public delegate void CustomerWasConsumedEventHandler();

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Clock.Instance.IsPaused) return;
		
		float amountToWalk = speed * (float)delta;

		if(stayDelay <= 0)
		{
			Want = GD.RandRange(0,1000);
			GD.Print("My New Want Is " + Want);
			stayDelay = Stay;
		}


		//GD.Print(firstDestinationReached+":"+secondDestinationReached+":"+finalDestinationReached);

			if(firstDestination == Vector2.Zero)
				firstDestinationReached = true;
			else if(secondDestination == Vector2.Zero)
				secondDestinationReached = true;
			else if(finalDestination == Vector2.Zero)
				finalDestinationReached = true;

		
		if (!firstDestinationReached)
		{
			SetPosition(GetPosition().MoveToward(firstDestination, amountToWalk));
			firstDestinationReached = GetPosition() == firstDestination;
		}
		else if (!secondDestinationReached)
		{
			SetPosition(GetPosition().MoveToward(secondDestination, amountToWalk));
			secondDestinationReached = GetPosition() == secondDestination;
		}
		else if (!finalDestinationReached)
		{
			SetPosition(GetPosition().MoveToward(finalDestination, amountToWalk));
			finalDestinationReached = GetPosition() == finalDestination;
		}
		else if (killAtFinalDestination)
		{
			EmitSignal(SignalName.CustomerWasConsumed);
			QueueFree();
		}


		if(Patience <= 0)
		{
			desiredMachine = null;
			GoHome();
		}
		else if(desiredMachine == null) // If no machine is chosen skip this
		{
			desiredMachine = Casino.GiveNewMachine(this);
			return;
		}
		else if(desiredMachine.playing == true || desiredMachine.refractoring == true) // If the machine is being played or resetting itself skip
			return;
		else if(finalDestinationReached == true) // Are we actually in front of the machine
		{
			if(desiredMachine.Attractiveness < -(Mood / 10) + Want) // Mood puts into the negative, Want value puts into positives, if Want is higher than Attractiveness they move machines
			{
				GD.Print("MY MACHINE IS ONLY " + desiredMachine.Attractiveness + " and I am craving " + (-(Mood / 10) + Want));
				Patience--;
				GD.Print("MY PATIENCE IS " + Patience);
				desiredMachine.PlayGame -= OnPlayGameSignal;
				desiredMachine = null;
			} else
			{
			stayDelay--;
			GD.Print(stayDelay);
			desiredMachine.PlayGame += OnPlayGameSignal;
			desiredMachine.PlayLottoGame();
			}

		}


	}

	public void OnVIPToggledSignal(bool toggled)
	{
		if (toggled)
		{
			selfButton.Disabled = false;
		}
		else
		{
			sprite.Visible = true;
			sprite2.Visible = false;
			Scale = new Vector2(1f,1f);
		}
	}
	
	private void OnMouseEntered()
	{
		if (selfButton.Disabled) return;
		sprite.Visible = false;
		sprite2.Visible = true;
			Scale = new Vector2(1.2f, 1.2f);
	}
	
	private void OnMouseExited()
	{
		if (selfButton.Disabled) return;
		sprite.Visible = true;
		sprite2.Visible = false;
			Scale = new Vector2(1f,1f);
	}
	
	private void OnMouseClicked()
	{
		firstDestinationReached = currentCounterAt == null;
		if (!firstDestinationReached)
			firstDestination = ((CustomerTravelRegion)currentCounterAt.GetNode<CollisionShape2D>(PATH_TRAVEL_REGION)).GetRandomPoint();
		secondDestinationReached = true;
		finalDestination = new Vector2(950, 140); // lmfao hardcoded parlay door location |||| hardcodeeznuts
		finalDestinationReached = false;
		killAtFinalDestination = true;
	}

	private void OnPlayGameSignal(int payouted)
	{
		Mood -= payouted;
		GD.Print("I Feel like " + Mood);
		desiredMachine.PlayGame -= OnPlayGameSignal;
	}

	
	private Vector2 firstDestination;
	private bool firstDestinationReached;
	private Vector2 secondDestination;
	private bool secondDestinationReached;
	private Vector2 finalDestination;
	private bool finalDestinationReached;
	private bool killAtFinalDestination = false; // killed at casino
	private bool leaveAtFinalDestination = false; // leave casino
	
	private Counter currentCounterAt;
	
	public bool IsWalking => !firstDestinationReached || !secondDestinationReached || !finalDestinationReached;
	
	private const string PATH_TRAVEL_REGION = "Area2D/CustomerTravelRegion";
	public void ChooseMachineToWalkTo(Counter counter, LottoMachine machine)
	{		

		if(machine == null)
		return;

		finalDestination = ((CustomerTravelRegion)machine.GetNode<CollisionShape2D>(PATH_TRAVEL_REGION)).GetRandomPoint();
		finalDestinationReached = false;
		
		// first see if the machine is on the same row we're already on
		if (Math.Abs(finalDestination.Y - Position.Y) < 150)
		{
			firstDestinationReached = true;
			secondDestinationReached = true;
			return;	
		}
		// if not, set first destination to the right and second destination to the counter
		else
		{
			firstDestinationReached = false;
			Counter toGo = currentCounterAt == null ? counter : currentCounterAt;
			firstDestination = ((CustomerTravelRegion)toGo.GetNode<CollisionShape2D>(PATH_TRAVEL_REGION)).GetRandomPoint();
			
			if (currentCounterAt != null)
			{
				secondDestination = ((CustomerTravelRegion)counter.GetNode<CollisionShape2D>(PATH_TRAVEL_REGION)).GetRandomPoint();
				secondDestinationReached = false;
			}
		}





		desiredMachine = machine;
		currentCounterAt = counter;
	}


		public void GoHome()
		{
			finalDestination = new Vector2(1100,800);
			if(this.Position != finalDestination)
			finalDestinationReached = false;

			if(finalDestinationReached)
			{
				GD.Print("I LEAVE NOW");
				Casino.customerCount--;
				QueueFree();

			}

		}




}
