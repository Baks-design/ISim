using Godot;

namespace ISim.UI
{
	public partial class Reticle : CenterContainer
	{
		[ExportCategory("Reticle")]
		[ExportGroup("Nodes")]
		[Export] public Polygon2D dot;

		public CharacterBody3D Character { get; set; }

		public override void _Ready() => dot = GetNode<Polygon2D>("dot");
	}
}