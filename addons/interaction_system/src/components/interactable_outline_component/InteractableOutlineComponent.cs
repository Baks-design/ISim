using System.Collections.Generic;
using Godot;

namespace InteractionSystem
{
    [Tool]
    public partial class InteractableOutlineComponent : Node
    {
    	Interactable _prop;
    	Node _outline;
    	MeshInstance2D _outline2D;
    	MeshInstance3D _outline3D;

    	[Export] public bool Enabled { get; set; } = true;
    	[Export]
    	public Node Outline
    	{
    		get => _outline;
    		set
    		{
    			if (value != _outline)
    			{
    				_outline = value;
    				UpdateConfigurationWarnings();
    			}
    		}
    	}
    	[Export]
    	public Interactable Prop
    	{
    		get => _prop;
    		set
    		{
    			if (value != _prop)
    			{
    				_prop = value;
    				UpdateConfigurationWarnings();
    			}
    		}
    	}

    	[ExportGroup("Outline On")]
    	[Export] public bool OutlineOnFocus { get; set; } = true;
    	[Export] public bool OutlineOnClosest { get; set; } = true;

    	public override void _Ready()
    	{
    		if (Engine.IsEditorHint())
    			return;

    		Prop.Focused += OnFocus;
    		Prop.Unfocused += OnFocusLost;
    		Prop.Closest += OnClosest;
    		Prop.NotClosest += OnNotClosest;

    		InitializeOutline();

    		HideOutline();
    	}

    	public override string[] _GetConfigurationWarnings()
    	{
    		var warnings = new List<string>();

    		if (Outline is null)
    			warnings.Add("Outline is null");
    		else if (Outline is not MeshInstance2D && Outline is not MeshInstance3D)
    			warnings.Add("Outline is not a MeshInstance2D or MeshInstance3D");

    		if (Prop is null)
    			warnings.Add("Prop is null");
    		else if (Prop is null)
    			warnings.Add("Prop is not an Interactable");

    		return warnings.ToArray();
    	}

    	void InitializeOutline()
    	{
    		if (Outline is MeshInstance2D)
    			_outline2D = Outline as MeshInstance2D;
    		else if (Outline is MeshInstance3D)
    			_outline3D = Outline as MeshInstance3D;
    	}

    	void ShowOutline()
    	{
    		_outline2D?.Show();
    		_outline3D?.Show();
    	}

    	void HideOutline()
    	{
    		_outline2D?.Hide();
    		_outline3D?.Hide();
    	}

    	void OnFocus(Interactor interactor)
    	{
    		if (OutlineOnFocus)
    			ShowOutline();
    	}

    	void OnFocusLost(Interactor interactor)
    	{
    		if (OutlineOnFocus)
    			HideOutline();
    	}

    	void OnClosest(Interactor interactor)
    	{
    		if (OutlineOnClosest)
    			ShowOutline();
    	}

    	void OnNotClosest(Interactor interactor)
    	{
    		if (OutlineOnClosest)
    			HideOutline();
    	}
    }
}

