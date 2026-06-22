using Godot;
using System;

public partial class Customer : Node2D
{
	public int Stubborness;
	
	private LottoMachine desiredMachine;

	private Button selfButton;
	private Sprite2D sprite;
	private Sprite2D sprite2;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Stubborness = GD.RandRange(1000, 10000);
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

		// debug text GD.Print(firstDestinationReached+":"+secondDestinationReached+":"+finalDestinationReached);
		
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

		if(desiredMachine == null)
		{
			return;
		}
		else if(desiredMachine.playing == true)
		{
			return;
		}
		else if(finalDestinationReached == true)
		{

			desiredMachine.PlayGame += OnPlayGameSignal; // TO DO: ACTUAL SELECTION MECHANICS AAAAAGH

			desiredMachine.PlayLottoGame();

			//GD.Print("My Stubborn is " + Stubborness);
		}


	}

	private void OnVIPToggledSignal(bool toggled)
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
		firstDestination = ((CustomerTravelRegion)currentCounterAt.GetNode<CollisionShape2D>(PATH_TRAVEL_REGION)).GetRandomPoint();
		firstDestinationReached = false;
		secondDestinationReached = true;
		finalDestination = new Vector2(950, 140); // lmfao hardcoded parlay door location
		finalDestinationReached = false;
		killAtFinalDestination = true;
	}

	private void OnPlayGameSignal(int payouted)
	{
		Stubborness -= payouted;
		//GD.Print("I Feel like " + Stubborness);

	}

	
	private Vector2 firstDestination;
	private bool firstDestinationReached = true;
	private Vector2 secondDestination;
	private bool secondDestinationReached = true;
	private Vector2 finalDestination;
	private bool finalDestinationReached = true;
	private bool killAtFinalDestination = false; // killed at casino
	private bool leaveAtFinalDestination = false; // leave casino
	
	private Counter currentCounterAt;
	
	public bool IsWalking => !firstDestinationReached || !secondDestinationReached || !finalDestinationReached;
	
	private const string PATH_TRAVEL_REGION = "Area2D/CustomerTravelRegion";
	public void ChooseMachineToWalkTo(Counter counter, LottoMachine machine)
	{

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





}
