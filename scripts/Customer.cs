using Godot;
using System;

public partial class Customer : Node2D
{
	public int Stubborness;
	
	LottoMachine desiredMachine;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
	
	private float speed = 100;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

		if (finalDestinationReached && desiredMachine.playing == false)
		{
			desiredMachine.PlayLottoGame();
		}




	}



	
	private Vector2 firstDestination;
	private bool firstDestinationReached = true;
	private Vector2 secondDestination;
	private bool secondDestinationReached = true;
	private Vector2 finalDestination;
	private bool finalDestinationReached = true;
	
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
