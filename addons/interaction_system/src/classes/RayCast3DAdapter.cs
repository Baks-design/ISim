#nullable enable

using Godot;
using InteractionSystem.Interfaces;

namespace InteractionSystem.Classes
{
    public class RayCast3DAdapter : IRayCast
    {
    	readonly RayCast3D _rayCast3D;

        public RayCast3DAdapter(RayCast3D rayCast3D) => _rayCast3D = rayCast3D;

        public Node? GetCollider() => (Node?)_rayCast3D.GetCollider();
    }
}

