

using System.Collections.Generic;
using Godot;

public partial class DialogueOption
{
    public TextAbsatz Text;
    public DialogueChoice Button;

    public List<DialogueOption> DialogueOptions;

    public DialogueOption ParentOption { get; private set; } // DOE I NEED TO BE A REFFERENCE?

    public int TabSeatNumber = 0;

    public bool IsAlreadyInDialogueParts = false;

    public static DialogueOption NewDialogueOption()
    {
        DialogueOption _dialogOpt = new DialogueOption();
		_dialogOpt.Button = new DialogueChoice();
		_dialogOpt.DialogueOptions = new List<DialogueOption>();
		var _packed = GD.Load<PackedScene>("res://dialogue/text_absatz.tscn");
		_dialogOpt.Text = (TextAbsatz)_packed.Instantiate();
        return _dialogOpt;
    }
}