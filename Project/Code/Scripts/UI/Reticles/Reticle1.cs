using Godot;

namespace ISim.UI
{
	public partial class Reticle1 : Reticle
	{
		[ExportCategory("Reticle")]
		[ExportGroup("Nodes")]
		[Export] Line2D[] reticleLines = new Line2D[4];

		[ExportGroup("Animate")]
		[Export] bool animatedReticle = true;
		[Export] float reticleSpeed = 0.5f;
		[Export] float reticleSpread = 4f;

		[ExportGroup("Dot Settings")]
		[Export] int dotSize = 1;
		[Export] Color dotColor = Colors.White;

		[ExportGroup("Line Settings")]
		[Export] Color lineColor = Colors.White;
		[Export] int lineWidth = 2;
		[Export] int lineLength = 10;
		[Export] int lineDistance = 5;
		[Export(PropertyHint.Enum, "None,Round")] int capMode = 0;

		public override void _Process(double delta)
		{
			if (Visible)
			{
				UpdateReticleSettings();
				if (animatedReticle)
					AnimateReticleLines();
			}
		}

		void AnimateReticleLines()
		{
			var velocity = Character.GetRealVelocity();
			var origin = Vector3.Zero;
			var pos = Vector2.Zero;
			var speed = origin.DistanceTo(velocity);

			reticleLines[0].Position = reticleLines[0].Position.Lerp(pos + new Vector2(0f, -speed * reticleSpread), reticleSpeed);
			reticleLines[1].Position = reticleLines[1].Position.Lerp(pos + new Vector2(-speed * reticleSpread, 0f), reticleSpeed);
			reticleLines[2].Position = reticleLines[2].Position.Lerp(pos + new Vector2(speed * reticleSpread, 0f), reticleSpeed);
			reticleLines[3].Position = reticleLines[3].Position.Lerp(pos + new Vector2(0f, speed * reticleSpread), reticleSpeed);
		}

		void UpdateReticleSettings()
		{
			Vector2 newScale = default;
			newScale = newScale with { X = dot.Scale.X, Y = dot.Scale.Y };
			dot.Scale = newScale;

			//Lines
			for (var i = 0; i < reticleLines.Length; i++)
			{
				reticleLines[i].DefaultColor = lineColor;
				reticleLines[i].Width = lineWidth;
				reticleLines[i].BeginCapMode = capMode is 0 ? Line2D.LineCapMode.None : Line2D.LineCapMode.Round;
				reticleLines[i].EndCapMode = capMode is 0 ? Line2D.LineCapMode.None : Line2D.LineCapMode.Round;
			}
		}
	}
}