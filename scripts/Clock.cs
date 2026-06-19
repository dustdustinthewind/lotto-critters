using Godot;
using System;

public partial class Clock : Node
{
	public static Clock Instance { get; private set; }
	
	public ulong PlayTimeElapsed
	{
		get
		{
			ulong additionalTime = 0;
			if (!paused)
			additionalTime = CurrentEngineTime - lastUnpauseStartTime;
			return timeElapsedBeforePause + additionalTime;
		}
	}

	public ulong PlayTimeElapsedInSeconds
	{
		get => PlayTimeElapsed / 1000;
	}
	
	private ulong lastFullSecond = 0;

	private bool paused = true;

	public bool IsPaused {get => paused;}

	private ulong timeElapsedBeforePause = 0;
	private ulong lastUnpauseStartTime = 0;

	public ulong CurrentEngineTime => Time.GetTicksMsec();

	// Starts or restarts clock
	public void RestartClock()
	{
		lastUnpauseStartTime = CurrentEngineTime;
		timeElapsedBeforePause = 0;
		paused = false;
	}

	public void ResumeClock()
	{
		lastUnpauseStartTime = CurrentEngineTime;
		paused = false;
	}

	public void PauseClock()
	{
		timeElapsedBeforePause = timeElapsedBeforePause + CurrentEngineTime - lastUnpauseStartTime;
		paused = true;
	}

	public string PlayTimeAsString()
	{
		TimeSpan t = TimeSpan.FromSeconds(PlayTimeElapsedInSeconds);
		return string.Format("{0:D2}:{1:D2}:{2:D2}",
		t.Hours,
		t.Minutes,
		t.Seconds);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}
	
	// emits every time a signal has passed
	[Signal]
	public delegate void TickTockEventHandler();
	
	public override void _Process(double delta)
	{
		if (PlayTimeElapsedInSeconds > lastFullSecond)
		{
			lastFullSecond = PlayTimeElapsedInSeconds;
			EmitSignal(SignalName.TickTock);
		}
	}
}
