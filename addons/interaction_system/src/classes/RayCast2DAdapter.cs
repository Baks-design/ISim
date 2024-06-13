#nullable enable

using Godot;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Classes
{
    public class RayCast2DAdapter : IRayCast
    {
    	readonly RayCast2D _rayCast2D;

        public RayCast2DAdapter(RayCast2D rayCast2D) => _rayCast2D = rayCast2D;

        public Node? GetCollider() => (Node?)_rayCast2D.GetCollider();
    }
}

