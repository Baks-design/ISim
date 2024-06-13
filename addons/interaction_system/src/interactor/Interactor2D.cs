#nullable enable

using System.Collections.Generic;
using Godot;
using InteractionSystem.Classes;

namespace InteractionSystem
{
    [Tool]
    public partial class Interactor2D : Interactor
    {
    	protected Area2D? _area;
    	protected RayCast2D? _rayCast;

    	[Export]
    	public RayCast2D? RayCast
    	{
    		get => _rayCast;
    		set
    		{
    			if (value != _rayCast)
    			{
    				_rayCast = value;
    				UpdateConfigurationWarnings();
    			}
    		}
    	}
    	[Export]
    	public Area2D? Area
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
    		base._Ready();

    		_adapter = _rayCast is not null ? new RayCast2DAdapter(_rayCast) : null;
    	}

    	public override string[] _GetConfigurationWarnings()
    	{
    		List<string> warnings = new();

    		if (_rayCast is null && _area is null)
    		{
    			var warning = "This node does not have the ability to interact with the world. " +
    				"Please add a RayCast2D or Area2D to this node.";
    			warnings.Add(warning);
    		}

    		warnings.AddRange(base._GetConfigurationWarnings() ?? System.Array.Empty<string>());

    		return warnings.ToArray();
    	}

    	public Interactable2D? GetClosestInteractable()
    	{
    		if (Area is null)
    			return null;

    		var list = Area.GetOverlappingAreas();
    		float distance;
    		var closestDistance = float.MaxValue;
    		Interactable2D? closestInteractable = null;

    		if (list.Count is 0)
    			return null;

    		foreach (var body in list)
    		{
    			var meta = body.GetMeta("interactable").As<NodePath>();
    			var interactable = GetInteractableFromPath(meta);

    			if (interactable is not Interactable2D)
    				continue;

    			distance = body.GlobalPosition.DistanceTo(Area.GlobalPosition);
    			if (distance < closestDistance)
    			{
    				closestDistance = distance;
    				closestInteractable = (Interactable2D)interactable;
    			}
    		}

    		return closestInteractable;
    	}
    }
}

