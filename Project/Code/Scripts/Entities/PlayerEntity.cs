using Godot;
using ISim.UI;
using ISim.Utils;

namespace ISim.Entities
{
	public partial class PlayerEntity : CharacterBody3D
	{
		enum State { Normal, Crouching, Sprinting }
		State state = State.Normal;

		[ExportCategory("Character")]
		[Export] float baseSpeed = 3f;
		[Export] float sprintSpeed = 6f;
		[Export] float crouchSpeed = 1f;
		[Export] float acceleration = 10f;
		[Export] float jumpVelocity = 4.5f;
		[Export] float mouseSensitivity = 0.005f;
		[Export] bool immobile = false;
		[Export(PropertyHint.File)] public string defaultReticle;
		[Export] Vector3 initialFacingDirection = Vector3.Zero;

		[ExportGroup("Nodes")]
		[Export] Node3D HEAD;
		[Export] Camera3D CAMERA;
		[Export] AnimationPlayer HEADBOB_ANIMATION;
		[Export] AnimationPlayer JUMP_ANIMATION;
		[Export] AnimationPlayer CROUCH_ANIMATION;
		[Export] CollisionShape3D COLLISION_MESH;
		[Export] ShapeCast3D CEILING_DETECTION;

		[ExportGroup("Controls")]
		[Export] string JUMP = "jump";
		[Export] string LEFT = "left";
		[Export] string RIGHT = "right";
		[Export] string FORWARD = "forward";
		[Export] string BACKWARD = "backward";
		[Export] string PAUSE = "pause";
		[Export] string CROUCH = "crouch";
		[Export] string SPRINT = "sprint";

		[ExportGroup("Feature Settings")]
		[Export] bool jumpingEnabled = true;
		[Export] bool inAirMomentum = true;
		[Export] bool motionSmoothing = true;
		[Export] bool sprintEnabled = true;
		[Export] bool crouchEnabled = true;
		[Export(PropertyHint.Enum, "Hold to Crouch,Toggle Crouch")] public int crouchMode = 0;
		[Export(PropertyHint.Enum, "Hold to Sprint,Toggle Sprint")] public int sprintMode = 0;
		[Export] bool dynamicFOV = true;
		[Export] bool continuousJumping = true;
		[Export] bool viewBobbing = true;
		[Export] bool jumpAnimation = true;

		// Member variables
		float speed;
		float currentSpeed = 0.0f;
		bool lowCeiling = false; // This is for when the ceiling is too low and the player needs to crouch.
		bool wasOnFloor = true;
		Reticle RETICLE;
		Control USER_INTERFACE;
		DebugPanel DEBUG_PANEL;
		// Get the gravity from the project settings to be synced with RigidBody nodes.
		public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

		public override void _Ready()
		{
			base._Ready();
			speed = baseSpeed;
			USER_INTERFACE = GetNode<Control>("UserInterface");
			DEBUG_PANEL = USER_INTERFACE.GetNode<Control>("DebugPanel") as DebugPanel;

			Input.MouseMode = Input.MouseModeEnum.Captured;

			// Set the camera rotation to whatever initial_facing_direction is, as long as it's not Vector3.zero
			if (!initialFacingDirection.Equals(Vector3.Zero))
				HEAD.RotationDegrees = initialFacingDirection;

			if (defaultReticle != null)
				ChangeReticle(defaultReticle);

			HEADBOB_ANIMATION.Play("RESET");
			JUMP_ANIMATION.Play("RESET");
			CROUCH_ANIMATION.Play("RESET");
		}

		public override void _PhysicsProcess(double delta)
		{
			currentSpeed = Vector3.Zero.DistanceTo(GetRealVelocity());

			DEBUG_PANEL.AddProperty("Speed", $"{currentSpeed:0.000}", 1);
			DEBUG_PANEL.AddProperty("Target Speed", $"{speed}", 2);
			var cv = GetRealVelocity();
			DEBUG_PANEL.AddProperty("Velocity", $"X: {cv.X:0.000} Y: {cv.Y:0.000} X: {cv.X:0.000}", 3);

			// Gravity
			//  If the gravity changes during your game, uncomment this code
			// gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
			HandleGravityAndJumping(delta);

			var inputDir = immobile ? Vector2.Zero : Input.GetVector(LEFT, RIGHT, FORWARD, BACKWARD);
			HandleMovement(delta, inputDir);

			lowCeiling = CEILING_DETECTION.IsColliding();
			HandleState(!inputDir.Equals(Vector2.Zero));

			if (dynamicFOV)
				UpdateCameraFOV();

			if (viewBobbing)
				HeadbobAnimation(!inputDir.Equals(Vector2.Zero));

			if (jumpAnimation)
			{
				if (!wasOnFloor && IsOnFloor()) // Just Landed
					JUMP_ANIMATION.Play((GD.Randi() % 2) is 1 ? "land_left" : "land_right");
				wasOnFloor = IsOnFloor(); //This must always be at the end of physics_process
			}
		}

		public override void _Process(double delta)
		{
			DEBUG_PANEL.AddProperty("FPS", $"{Performance.GetMonitor(Performance.Monitor.TimeFps)}", 0);
			DEBUG_PANEL.AddProperty("state", $"{state}" + (!IsOnFloor() ? " in the air" : ""), 4);

			if (Input.IsActionJustPressed(PAUSE))
				Input.MouseMode = (Input.MouseMode is Input.MouseModeEnum.Captured) ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventMouseMotion motion && Input.MouseMode is Input.MouseModeEnum.Captured)
			{
				var iemm = motion;
				var currentRotation = HEAD.Rotation;
				currentRotation.Y -= iemm.Relative.X * mouseSensitivity;
				currentRotation.X -= iemm.Relative.Y * mouseSensitivity;
				HEAD.Rotation = currentRotation;
			}
		}

		#region States
		void HandleState(bool moving)
		{
			if (sprintEnabled)
			{
				if (sprintMode is 0)
				{
					if (Input.IsActionPressed(SPRINT) && state is not State.Crouching)
					{
						if (moving && state is not State.Sprinting)
							EnterSprintState();
						else if (!moving && state is State.Sprinting)
							EnterNormalState();
					}
					else if (state is State.Sprinting)
						EnterNormalState();
				}
				else if (sprintMode is 1)
				{
					if (moving)
					{
						if (Input.IsActionPressed(SPRINT) && state is State.Normal)
							EnterSprintState();
						if (Input.IsActionJustPressed(SPRINT))
						{
							switch (state)
							{
								case State.Normal:
									EnterSprintState();
									break;
								default:
									EnterNormalState();
									break;
							}
						}
					}
					else if (state is State.Sprinting)
						EnterNormalState();
				}
			}

			if (crouchEnabled)
			{
				if (crouchMode is 0)
				{
					if (Input.IsActionPressed(CROUCH) && state is not State.Sprinting)
					{
						if (state is not State.Crouching)
							EnterCrouchState();
					}
					else if (!CEILING_DETECTION.IsColliding() && state is State.Crouching)
						EnterNormalState();
				}
				else if (crouchMode is 1)
				{
					if (Input.IsActionJustPressed(CROUCH))
					{
						switch (state)
						{
							case State.Normal:
								EnterCrouchState();
								break;
							default:
								if (!CEILING_DETECTION.IsColliding())
									EnterNormalState();
								break;
						}
					}
				}
			}
		}

		void EnterNormalState()
		{
			var previousState = state;
			if (previousState is State.Crouching)
				CROUCH_ANIMATION.PlayBackwards("crouch");
			state = State.Normal;
			speed = baseSpeed;
		}

		void EnterSprintState()
		{
			var previousState = state;
			if (previousState is State.Crouching)
				CROUCH_ANIMATION.PlayBackwards("crouch");
			state = State.Sprinting;
			speed = sprintSpeed;
		}

		void EnterCrouchState()
		{
			state = State.Crouching;
			speed = crouchSpeed;
			CROUCH_ANIMATION.Play("crouch");
		}
		#endregion

		#region Movement
		void HandleGravityAndJumping(double delta)
		{
			var currentVelocity = Velocity;
			if (!IsOnFloor())
				currentVelocity.Y -= (float)(gravity * delta);
			else if (jumpingEnabled)
			{
				if (continuousJumping ? Input.IsActionPressed(JUMP) : Input.IsActionJustPressed(JUMP))
				{
					if (IsOnFloor() && !lowCeiling)
					{
						JUMP_ANIMATION?.Play("jump");
						currentVelocity.Y += jumpVelocity;
					}
				}
			}
			Velocity = currentVelocity;
		}

		void HandleMovement(double delta, Vector2 inputDir)
		{
			var direction2D = inputDir.Rotated(-HEAD.Rotation.Y);
			Vector3 direction = default;
			direction = direction with { X = direction2D.X, Y = 0f, Z = direction2D.Y };
			direction = direction.Normalized();
			MoveAndSlide();

			if (!inAirMomentum || IsOnFloor())
			{
				Velocity = Velocity with
				{
					X = motionSmoothing ? Mathf.Lerp(Velocity.X, direction.X * speed, (float)(acceleration * delta)) : direction.X * speed,
					Z = motionSmoothing ? Mathf.Lerp(Velocity.Z, direction.Z * speed, (float)(acceleration * delta)) : direction.Z * speed
				};
			}
		}
		#endregion

		#region Visuals
		void ChangeReticle(string reticlePath)
		{
			RETICLE?.QueueFree();
			RETICLE = GD.Load<PackedScene>(reticlePath).Instantiate<Reticle>();
			RETICLE.Character = this;
			USER_INTERFACE.AddChild(RETICLE);
		}

		void UpdateCameraFOV() => CAMERA.Fov = Mathf.Lerp(CAMERA.Fov, state is State.Sprinting ? 85f : 75f, 0.3f);

		void HeadbobAnimation(bool moving)
		{
			if (moving && IsOnFloor())
			{
				var useHeadbobAnimation = (state is State.Normal || state is State.Crouching) ? "walk" : "sprint";
				var wasPlaying = HEADBOB_ANIMATION.CurrentAnimation == useHeadbobAnimation;

				HEADBOB_ANIMATION.Play(useHeadbobAnimation, 0.25f);
				HEADBOB_ANIMATION.SpeedScale = currentSpeed / baseSpeed * 1.75f;
				if (!wasPlaying)
					HEADBOB_ANIMATION.Seek((double)(GD.Randi() % 2f));
			}
			else
			{
				HEADBOB_ANIMATION.Play("RESET", 0.25f);
				HEADBOB_ANIMATION.SpeedScale = 1f;
			}
		}
		#endregion
	}
}