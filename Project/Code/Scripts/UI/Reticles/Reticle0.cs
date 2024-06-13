using Godot;

namespace ISim.UI
{
	public partial class Reticle0 : Reticle
	{
		[ExportGroup("Settings")]
		[Export] int dotSize = 1;
		[Export] Color dotColor = Colors.White;

		public override void _Process(double delta)
		{
			if (Visible)
				UpdateReticleSettings();
		}

		void UpdateReticleSettings()
		{
			if (dot.GetType() == typeof(Polygon2D))
			{
				dot.Scale = dot.Scale with { X = dotSize, Y = dotSize };
				dot.Color = dotColor;
			}
		}
	}
}