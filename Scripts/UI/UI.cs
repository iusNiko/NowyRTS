using Godot;
using System;
using System.Data;
using System.Security.Cryptography.X509Certificates;

public partial class UI : CanvasLayer
{
	public static UI Instance;
	[Export] public int ButtonSizePixels = 16;
	[Export] public int DistanceBetweenButtons = 2;
	[Export] public float CommandCardScale = 3;
	[Export] Vector2I CommandCardSize = new(6, 4);
	public bool IsMouseOverCommandCard = false;
	public Button[,] CommandCardButtons;
	public Panel CommandCard;
	public Panel QueueUI;

	public void ClearCommandCard() {
		foreach(Button button in CommandCard.GetChildren()) {
			button.QueueFree();
		}
		for(int y = 0; y < CommandCardSize.Y; y++) {
			for(int x = 0; x < CommandCardSize.X; x++) {
				CommandCardButtons[x, y] = null;
			}
		}
	}

	public void CreateQueueUI(int itemCount, int itemSize, int distanceBetweenItems) {
		if(QueueUI != null && IsInstanceValid(QueueUI)) {
			QueueUI.QueueFree();
		}
		QueueUI = new Panel();
		AddChild(QueueUI);
		QueueUI.Size = new Vector2(itemSize * itemCount + distanceBetweenItems * (itemCount + 1), 
		 itemSize + distanceBetweenItems * 2);
		QueueUI.Scale = new Vector2(CommandCardScale, CommandCardScale);

		int i = 0;
		foreach(Ability abil in World.Instance.LastUnitSelected.AbilityQueue) {
			abil.CreateQueueButton(new Vector2I(i * itemSize + (i+1) * distanceBetweenItems, distanceBetweenItems));
			i++;
		}
	}

	public Vector2I GetNearestEmptyButtonSpace() {
		for(int y = 0; y < CommandCardSize.Y; y++) {
			for(int x = 0; x < CommandCardSize.X; x++) {
				if(CommandCardButtons[x, y] == null) return new Vector2I(x, y);
			}
		}
		return new Vector2I(-1, -1);
	}

	public Vector2I ButtonPosToCommandCardPos(Vector2I buttonPos) {
		return new Vector2I(buttonPos.X * ButtonSizePixels + (buttonPos.X + 1) * DistanceBetweenButtons, 
		 buttonPos.Y * ButtonSizePixels + (buttonPos.Y + 1) * DistanceBetweenButtons);
	}
	public void CommandCardMouseEnetered() {
		IsMouseOverCommandCard = true;
	}

	public void CommandCardMouseExited() {
		IsMouseOverCommandCard = false;
	}

	public override void _Ready()
	{
		Instance = this;

		CreateCommandCard();
		CommandCard.MouseEntered += CommandCardMouseEnetered;
		CommandCard.MouseExited += CommandCardMouseExited;

		CommandCardButtons = new Button[CommandCardSize.X, CommandCardSize.Y];

		for(int y = 0; y < CommandCardSize.Y; y++) {
			for(int x = 0; x < CommandCardSize.X; x++) {
				CommandCardButtons[x, y] = null;
			}
		}
	}
    public override void _Process(double delta)
    {
		if(World.Instance.LastUnitSelected != null && World.Instance.LastUnitSelected.AbilityQueue.Count > 0) {
			CreateQueueUI(World.Instance.LastUnitSelected.AbilityQueueLength, 16, 2);
		}
		else {
			if(QueueUI != null && IsInstanceValid(QueueUI)) {
				QueueUI.QueueFree();
			}
		}

		CommandCardScale = (World.Instance.GetViewportRect().Size.X * 3) / 1920;
		CommandCard.Scale = new Vector2(CommandCardScale, CommandCardScale);
        CommandCard.Position = World.Instance.GetViewportRect().Size - CommandCard.Size * CommandCardScale;

		if(QueueUI != null && IsInstanceValid(QueueUI)) {
			QueueUI.Scale = new Vector2(CommandCardScale, CommandCardScale);
			QueueUI.Position = new Vector2(World.Instance.GetViewportRect().Size.X - CommandCard.Size.X * CommandCardScale - QueueUI.Size.X * CommandCardScale, 
			 World.Instance.GetViewportRect().Size.Y - QueueUI.Size.Y * CommandCardScale);
		}
    }

    public virtual void CreateCommandCard() {
		CommandCard = new Panel();
		AddChild(CommandCard);
		CommandCard.Size = new Vector2(ButtonSizePixels * CommandCardSize.X + DistanceBetweenButtons * (CommandCardSize.X + 1), 
		 ButtonSizePixels * CommandCardSize.Y + DistanceBetweenButtons * (CommandCardSize.Y + 1));
	}
}
