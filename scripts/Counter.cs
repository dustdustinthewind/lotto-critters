using Godot;
using System;

// Todo sort machines on counter
// Better sprite than just a polygon blegh

public partial class Counter : Node2D
{
	public LottoMachine[] Machines = new LottoMachine[4];
	
	public void AddMachine(LottoMachine machine)
	{
		for (int i = 0; i < Machines.Length; i++)
			if (Machines[i] == null)
			{
				Machines[i] = machine;
				GetNode<Node2D>(string.Format("MachineSlot{0}", i)).AddChild(Machines[i]);
				break;
			}
		
		// todo make signal
		GD.Print("COUNTER IS FULL, TRY ADDING SOMEWHERE ELSE");
	}
	
	public LottoMachine RemoveMachine(int index)
	{
		LottoMachine toReturn = Machines[index];
		Machines[index] = null;
		return toReturn;
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
