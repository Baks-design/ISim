using Godot;

namespace ISim.Utils
{
	public partial class DebugPanel : PanelContainer
	{
		VBoxContainer boxContainer;

		public override void _Ready() => boxContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer");

		public override void _Process(double delta) { }

		public void AddProperty(string title, string value, int order)
		{
			if (boxContainer.GetType().Equals(typeof(VBoxContainer)))
			{
				var target = boxContainer.FindChild(title, true, false);
				if (target == null)
				{
					var label = new Label { Name = title, Text = $"{title}: {value}" };
					boxContainer.AddChild(label);
				}
				else if (Visible)
				{
					var label = (Label)target;
					label.Text = $"{title}: {value}";
					boxContainer.MoveChild(target, order);
				}
			}
		}
	}
}