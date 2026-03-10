using Godot;
using System;
using DialogueManagerRuntime;
using System.Collections.Generic;


/// <summary>
/// Should absolutely always be a child of an individual node2D node specialized for it in the scenetree. Otherwise cannot find the Player node.
/// </summary>
public partial class DialogueObject : Node2D
{
	// Called when the node enters the scene tree for the first time.

	[Export] public Resource DialogueResource;

	[Export] public string DialogueStart = "start";

	private bool isPlayerInArea = false;
	private byte InOtherAreas; // checks whether player is in other areas to not make isplayerinarea go to false when exiting slightly one area
	private Godot.Collections.Dictionary<string, bool> EnteredBodies = new Godot.Collections.Dictionary<string, bool>
	{
		{"up", false },
		{"right", false },
		{"down", false },
		{"left", false}
	};
	public override void _Ready()
	{
		InOtherAreas = 0;
		GD.Print("Gay ballsack - dialogue object!");
		ConnectToPlayer(); // doesn't work down at the end of the function for some reason??????
		GD.Print("_DialogueObject's _Ready() is finished!");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void DownBodyEntered(Node2D body)
	{
		GD.Print("Down body entered");
		InOtherAreas += 1;
		isPlayerInArea = true;
		EnteredBodies["down"] = true;
	}
	public void DownBodyExited(Node2D body)
	{
		GD.Print("Down body exited");
		InOtherAreas -= 1;
		if (InOtherAreas == 0)
		{
			isPlayerInArea = false;
		}
		EnteredBodies["down"] = false;
	}
	public void UpBodyEntered(Node2D body)
	{
		GD.Print("Up body entered");
		InOtherAreas += 1;
		isPlayerInArea = true;
		EnteredBodies["up"] = true;
	}
	public void UpBodyExited(Node2D body)
	{
		GD.Print("Up body exited");
		InOtherAreas -= 1;
		if (InOtherAreas == 0)
		{
			isPlayerInArea = false;
		}
		EnteredBodies["up"] = false;
	}
	public void RightBodyEntered(Node2D body)
	{
		GD.Print("Right body entered");
		InOtherAreas += 1;
		isPlayerInArea = true;
		EnteredBodies["right"] = true;
	}
	public void RightBodyExited(Node2D body)
	{
		GD.Print("Right body exited");
		InOtherAreas -= 1;
		if (InOtherAreas == 0)
		{
			isPlayerInArea = false;
		}
		EnteredBodies["right"] = false;
	}
	public void LeftBodyEntered(Node2D body)
	{
		GD.Print("Left body entered");
		InOtherAreas += 1;
		isPlayerInArea = true;
		EnteredBodies["left"] = true;
	}
	public void LeftBodyExited(Node2D body)
	{
		GD.Print("Left body exited");
		InOtherAreas -= 1;
		if (InOtherAreas == 0)
		{
			isPlayerInArea = false;
		}
		EnteredBodies["left"] = false;
	}
	private void Action()
	{
		DialogueManager.ShowDialogueBalloon(DialogueResource, DialogueStart);
	}

	private void ConnectToPlayer()
	{
		GD.Print("Dialogue object connecting to player");
		if (GetParent().HasNode("../Player"))
		{
			GD.Print("Found the player node - dialogueobject.cs");
		}
		else
		{
			GD.Print("did NOT find the player node - dialogueobject.cs");
		}
		Player player = GetParent().GetNode<Player>("../Player"); // make this more pretty, this is really messy and accessing parent nodes is bad bad 
		player.Interact += PlayerInteract;
	}

	/// <Summary>
	/// Checks if the player faces the dialogue object and if true spawns a dialogue box. Otherwise nothing.
	/// </Summary>
	public void PlayerInteract(String currentState)
	{
		if (isPlayerInArea == true)
		{
			GD.Print("Checking if player is in atleast one of the areas");
			if ((EnteredBodies["down"] == true && currentState == "move_up") || (EnteredBodies["up"] == true && currentState == "move_down") || (EnteredBodies["right"] == true && currentState == "move_left") || (EnteredBodies["left"] == true && currentState == "move_right"))
			{
				GD.Print("Player has succesfully interacted");
				Action();
			}
		}
		GD.Print("Player has insuccesfully interacted");
	}
}
