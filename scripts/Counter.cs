using Godot;
using System;

// Todo sort machines on counter
// Better sprite than just a polygon blegh

public partial class Counter : Node2D
{
	public LottoMachine[] Machines = new LottoMachine[4];
	public Button[] Buttons = new Button[4];
	
	public bool AddMachine(LottoMachine machine)
	{
		for (int i = 0; i < Machines.Length; i++)
			if (AddMachine(machine, i))
				return true;
		
		// todo make signal?
		GD.Print("COUNTER IS FULL, TRY ADDING SOMEWHERE ELSE");
		return false;
	}
	
	public bool AddMachine(LottoMachine machine, int i)
	{
		if (Machines[i] == null)
		{
			Machines[i] = machine;
			Buttons[i].AddChild(Machines[i]);
			Buttons[i].Disabled = true;
			return true;
		}

		// todo make signal?
		GD.Print("SLOT IS FULL, TRY ADDING SOMEWHERE ELSE");
		return false;
	}
	
	public LottoMachine RemoveMachine(int index)
	{
		LottoMachine toReturn = Machines[index];
		Machines[index] = null;
		Buttons[index].RemoveChild(toReturn);
		Buttons[index].Disabled = false;
		// toReturn.QueueFree();
		return toReturn;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i] = GetNode<Button>(string.Format("MachineSlot" + i));
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
