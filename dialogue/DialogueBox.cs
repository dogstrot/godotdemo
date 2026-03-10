using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public partial class DialogueBox : Node2D
{

	[Export] public string TextFile { get; set; }
	public string SourceText { get; set; }
	private int TickIteration = 0;

	private Timer TickSpeed;

	private List<DialogueOption> DialogueParts;

	private DialogueOption CurrentDialogue;

	private bool DialogueTreeAlreadyApplied = false;

	private int CurrentDialogueIterator = 0;

	private int DialogueTreeIterator = 0;

	public override void _Ready()
	{
		TickSpeed = GetNode<Timer>("DialogueSpeed");
		TickSpeed.Timeout += OnDialogueTick;
		TickIteration = 0;
		CurrentDialogueIterator = 0;
		DialogueTreeAlreadyApplied = false;
		DialogueParts = new List<DialogueOption>();
		CurrentDialogue = new DialogueOption();
		ProcessTextFile();
		ReadyUpDialogueLists();
		TickSpeed.Start();
	}

	private void ProcessTextFile()
	{
		if (TextFile != null)
		{
			SourceText = File.ReadAllText(TextFile);
		}
	}

	/* bool _seatedChoiceFound = false;
		bool _choiceFinished = false;
		while (true)
		{
			if (_seatedChoiceFound == true)
			{
				while (SourceText[iter] == '\t')
				{
					_tabNumber++;
					++iter;
				}
			}
			DialogueOption _newChoice = DialogueOption.NewDialogueOption();
			_newChoice.TabSeatNumber = _tabNumber;
			while (true) // fill the button name
			{
				_newChoice.Button.Text += SourceText[iter++];
				if (SourceText[iter] == '\n')
				{
					break;
				}
			}
			while (true) // fill choice text */

/// <summary>
/// fills dialogueoption with button name and text, stops exactly as it hits a newline
/// </summary>
/// <param name="iter"></param>
/// <param name="tabNumber"></param>
/// <returns></returns>
	private DialogueOption OnChoiceFound(ref int iter, int tabNumber)
	{
		int _tabNumberAfterButton = 0;
		DialogueOption _newChoice = DialogueOption.NewDialogueOption();
		_newChoice.Button = new DialogueChoice();
		_newChoice.Button = (DialogueChoice)GD.Load<PackedScene>("res://dialogue/dialogue_choice.tscn").Instantiate();
		_newChoice.TabSeatNumber = tabNumber; // the => tab seat number, not text inhalt
		while (true) // fill the button name
		{
			_newChoice.Button.Text += SourceText[iter++];
			if (SourceText[iter] == '\n')
			{
				iter++;
				while (SourceText[iter] == '\t')
				{
					_tabNumberAfterButton++;
					++iter;
				}
				_newChoice.TabSeatNumber = _tabNumberAfterButton;
				break;
			}
		}
		while (true) // fill choice text
		{
			_newChoice.Text.Text += SourceText[iter++];
			if (SourceText[iter] == '\n') // new line found
			{
				_newChoice.Text.VisibleRatio = 0;
				return _newChoice;
			}
		}
	}

	private DialogueOption FindSeatedOptionParent(int seatedTabNumber)
	{
		int _iter = DialogueParts.Count-1;
		DialogueOption _currentOption = new DialogueOption();
		_currentOption = DialogueParts[_iter];
		while (true)
		{
			if (_currentOption.TabSeatNumber != seatedTabNumber-1)// if it's not the parent of the seated option
			{
				if (_currentOption.TabSeatNumber < seatedTabNumber) // move to child
				{
					_iter = _currentOption.DialogueOptions.Count-1;
					_currentOption = _currentOption.DialogueOptions[_iter];
				}
			}
			else if (_currentOption.TabSeatNumber == seatedTabNumber-1)
			{
				return _currentOption;
			}
		}
	}

	private void SkipToArrow(ref int iter)
	{
		if (SourceText[iter] == '\n')
		{
			++iter;
			if (SourceText[iter] == '\t')
			{
				while (SourceText[iter] == '\t')
				{
					++iter;
				}
			}
		}
	}

	private bool ContinueSeatedChoiceAdding(ref int iter)
	{
		int _diff = 0;
		if (SourceText[iter] == '\n')
		{
			++iter;
			if (SourceText[iter] == '\t')
			{
				while (SourceText[iter] == '\t')
				{
					++_diff;
					++iter;
				}
			}
			GD.Print(SourceText[iter]);
		}
		if (_diff <= 1)
		{
			return false;
		}
		else
		{
			--iter; // to counteract double ++iter in the while loop and and from this func
			return true;
		}
	}

	private async void ReadyUpDialogueLists()
	{
		int _iter = 0;
		bool _endOfWholeText = false;
		DialogueOption _parentText = DialogueOption.NewDialogueOption();
		while (_iter < SourceText.Length)
		{
			if (SourceText[_iter] == '\n') // when we are on a new line
			{
				int _tabNumber = 0;
				++_iter; // move on from iter
				if (_iter >= SourceText.Length)
				{
					break;
				}
				while (SourceText[_iter] == '\t')
				{
					_tabNumber++;
					++_iter;
				}
				if (SourceText[_iter] == '=' && SourceText[_iter + 1] == '>') // if a choice is found, keeps 
				{
					while (SourceText[_iter] == '=' && SourceText[_iter + 1] == '>')
					{
						DialogueOption _newChoiceFound = new DialogueOption();
						_newChoiceFound = OnChoiceFound(ref _iter, _tabNumber);
						_parentText.DialogueOptions.Add(_newChoiceFound);
						if (_iter >= SourceText.Length)
						{

							_endOfWholeText = true;
							break;
						}
						else if (_iter < SourceText.Length - 1)
						{
							if (SourceText[_iter] == '\n' && SourceText[_iter + 1] == '\t') // if seated choice
							{
								bool _continue = true;
								while (_continue == true)
								{
									_continue = false;
									//FIXME:: I need to be looped so i can support more seated options in a row
									++_iter; // CountTabs() does NOT include the \n
									int _seatedTabNumber = CountTabs(ref _iter);
									GD.Print(_seatedTabNumber);
									if (SourceText[_iter] == '=' && SourceText[_iter + 1] == '>')
									{
										DialogueOption _seatedOption = new DialogueOption();
										_seatedOption.TabSeatNumber = _seatedTabNumber;
										_seatedOption = OnChoiceFound(ref _iter, _seatedOption.TabSeatNumber);
										DialogueOption _seatedOptionsParent = new DialogueOption();
										if (_parentText.IsAlreadyInDialogueParts == false)
										{
											_parentText.IsAlreadyInDialogueParts = true;
											_parentText.Text.VisibleRatio = 0;
											DialogueParts.Add(_parentText);
										}
										_seatedOptionsParent = FindSeatedOptionParent(_seatedOption.TabSeatNumber);
										_seatedOptionsParent.DialogueOptions.Add(_seatedOption); //FIXME:: doesnt do ++iter of else, so it doesnt do the while loop
										_continue = ContinueSeatedChoiceAdding(ref _iter); // FIXME:: never adds the right amount to skip to =>
									}
								}
							}
							else
							{
								_iter += 1; //since we stop on the prelast character before \n
							}
						}
					}
					//_parentText.Text.VisibleRatio = 0;
					//DialogueParts.Add(_parentText);
					_parentText = DialogueOption.NewDialogueOption();
				}
			}
			if (_endOfWholeText == false)
			{
				_parentText.Text.Text += SourceText[_iter++];
			}
			else
			{
				break;
			}
		}
		if (_parentText.Text.Text != null && _parentText.Text.Text != "")
		{
			_parentText.Text.VisibleRatio = 0;
			DialogueParts.Add(_parentText);
		}
		CurrentDialogue = DialogueParts[DialogueTreeIterator];
		GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(CurrentDialogue.Text);
	}

	private int CountTabs(ref int iter)
	{
		int _tabNumber = 0;
		while (SourceText[iter] == '\t')
		{
			_tabNumber++;
			++iter;
		}
		return _tabNumber;
	}
	public async void OnDialogueTick()
	{
		if (TickIteration >= SourceText.Length)
		{
			TickSpeed.Stop();
			return;
		}
		else
		{
			TickIteration++;
		}
		if (CurrentDialogue.Text.VisibleRatio == 1)
		{
			TickSpeed.Stop();
			if (CurrentDialogue.DialogueOptions != null && CurrentDialogue.DialogueOptions.Count != 0 &&  DialogueTreeAlreadyApplied == false) // always goes into foreach loop over dialogue choices
			{
				foreach (DialogueOption c in CurrentDialogue.DialogueOptions)
				{
					c.Button.Pressed += () =>
					{
						foreach (var x in GetTree().GetNodesInGroup("dialogue choice"))
						{
							GD.Print(x.Name);
							x.QueueFree();
						}
						TextAbsatz _x;
						var _packed = GD.Load<PackedScene>("res://dialogue/text_absatz.tscn");
						_x = (TextAbsatz)_packed.Instantiate();
						_x.Text = c.Button.Text;
						GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(_x);
						c.Text.VisibleCharacters = 0;
						CurrentDialogue = c;
						TickSpeed.Start();
						GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(c.Text);
					};
					GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(c.Button);
				}
			}
			else if (CurrentDialogue.DialogueOptions.Count == 0 && DialogueTreeIterator < DialogueParts.Count-1)
			{
				++DialogueTreeIterator;
				CurrentDialogue = DialogueParts[DialogueTreeIterator]; // move on unto the next dialogue tree
				GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(CurrentDialogue.Text);
				TickSpeed.Start();
			}
			else if (CurrentDialogueIterator < DialogueParts.Count && CurrentDialogueIterator + 1 != DialogueParts.Count)
			{
				DialogueTreeAlreadyApplied = false;
				++CurrentDialogueIterator;
				CurrentDialogue = DialogueParts[CurrentDialogueIterator];
				GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(CurrentDialogue.Text);
				TickSpeed.Start();
			}
		}
		CurrentDialogue.Text.VisibleCharacters += 1;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	/*int _textIter = 0;
			bool _choiceFound = false;
			while (_textIter < SourceText.Length)
			{
				DialogueOption _sourceOpt = new DialogueOption();
				_sourceOpt.Button = new DialogueChoice();
				_sourceOpt.DialogueOptions = new List<DialogueOption>();
				var _packed = GD.Load<PackedScene>("res://dialogue/text_absatz.tscn");
				_sourceOpt.Text = (TextAbsatz)_packed.Instantiate();
				int _tabNumber = 0;
				while (_textIter < SourceText.Length) //add letters until a choice => was hit
				{
					if (SourceText[_textIter] == '=' && SourceText[_textIter + 1] == '>')
					{
						GD.Print("Found an arrow! - ReadyUpDialogueLists");
						_choiceFound = true;
						break;
					}
					_sourceOpt.Text.Text += SourceText[_textIter];
					++_textIter;
				}
				if (_choiceFound == true) //fill the choice button name and give it to dialogopt (NOT SEATED)
				{
					GD.Print("Bubbon found :P");
					while (_textIter < SourceText.Length) // fill the choice buttons names and add them as children to dialogueoption parent
					{
						DialogueOption _optFromButton = new DialogueOption();
						_optFromButton.Button = new DialogueChoice();
						_optFromButton.DialogueOptions = new List<DialogueOption>();
						_optFromButton.Text = (TextAbsatz)GD.Load<PackedScene>("res://dialogue/text_absatz.tscn").Instantiate();
						if (SourceText[_textIter] == '"')
						{
							++_textIter; // to avoid adding " at the start of the button text
							var _choice = GD.Load<PackedScene>("res://dialogue/dialogue_choice.tscn");
							_optFromButton.Button = (DialogueChoice)_choice.Instantiate();
							while (true)
							{
								_optFromButton.Button.Text += SourceText[_textIter];
								++_textIter;
								if (SourceText[_textIter] == '"')
								{
									bool _tabfound = false;
									while (_textIter < SourceText.Length)
									{
										if (SourceText[_textIter] == '\n' && _tabfound == true)
										{
											break;
										}
										if (SourceText[_textIter] == '\t')
										{
											_tabfound = true;
										}
										_optFromButton.Text.Text += SourceText[_textIter];
										++_textIter;
									}
									_sourceOpt.DialogueOptions.Add(_optFromButton);
									break;
								}
							}
						}
						++_textIter;
					}
				}
				_sourceOpt.Text.VisibleCharacters = 0;
				DialogueParts.Add(_sourceOpt);
			}
			CurrentDialogue = DialogueParts[0];
			GetNode<VBoxContainer>("ScrollContainer/VBoxContainer").AddChild(DialogueParts[0].Text);
			GD.Print("added the textc absatz");*/
}
