using Godot;
using System;

public partial class MoneyNotification : Label
{
	private float currentTransparency = 0.8f;
	private float transparencyDrainGain = 0.3f; // per second
	
	private float currentFontSize = 16;
	private float maxFontSize = 52;
	private float fontGain = 2f; // per second
	private float fontLoss = -0.15f; // per second
	private float fontGainDrain;
	
	private float speed = 300; // pixels per second
	
	private Color positiveColor => new Color(0, 176f/255f, 6f/255f, currentTransparency);
	private Color negativeColor => new Color(176f/255f, 0, 6f/255f, currentTransparency);
	private Color currentColor;
	
	private LabelSettings labelSettings = new LabelSettings();

	public int Money
	{
		get => money;
		set
		{
			money = value;
			currentColor = money > 0 ? positiveColor : negativeColor;
			Text = "$" + Math.Abs(money);
		}
	}
	private int money = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SetLabelSettings(labelSettings);
		
		fontGainDrain = fontGain;
		labelSettings.FontSize = (int)currentFontSize;
		
		labelSettings.SetFont((Font)GD.Load<Font>("res://fonts/ChelseaMarket-Regular.ttf"));
		
		Position -= new Vector2(-65, 40); // this sucks
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{		
		// move up
		Position += new Vector2(0, -speed * (float)delta);
		
		// gain or drain transparency
		if (currentTransparency >= 1f)
		{
			currentTransparency = 1f;
			transparencyDrainGain *= -1;
		}
		currentTransparency += transparencyDrainGain * (float)delta;
		Modulate = currentColor;
		
		// gain or lose font size
		if (currentFontSize >= maxFontSize)
		{
			currentFontSize = maxFontSize;
			fontGainDrain = fontLoss;
			speed = 2;
		}
		currentFontSize += fontGainDrain;
		GetLabelSettings().FontSize = (int)currentFontSize;
		
		if (currentFontSize <= 2 || currentTransparency <= 0.01f)
		{
			//GD.Print("Change da world: my final message. Goodbye.");
			this.QueueFree();
		}
	}
}
