#nullable enable

using System.Collections.Generic;
using Godot;

namespace InteractionSystem
{
    [Tool]
    public partial class Interactable3D : Interactable
    {
    	protected Area3D? _area;

    	[Export]
    	public Area3D? Area
    	{
    		get => _area;
    		set
    		{
    			if (value != _area)
    			{
    				_area = value;
    				UpdateConfigurationWarnings();
    			}
    		}
    	}

    	public override void _Ready()
    	{
    		if (Engine.IsEditorHint())
    			return;

    		Area?.SetMeta("interactable", GetPath());
    	}

    	public override string[] _GetConfigurationWarnings()
    	{
    		var warnings = new List<string>();

    		if (_area is null)
    		{
    			var warning = "This node does not have the ability to be interacted with. " +
    				"Please add an Area3D to this node.";
    			warnings.Add(warning);
    		}

    		warnings.AddRange(base._GetConfigurationWarnings() ?? System.Array.Empty<string>());

    		return warnings.ToArray();
    	}
    }
}

