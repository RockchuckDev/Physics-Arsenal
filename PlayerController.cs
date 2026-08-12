using Godot;
using System;
using System.Runtime.InteropServices.Marshalling;

public partial class PlayerController : RigidBody3D
{
    /// <summary> The rate at which the player accelerates (speeds up) when they are on the ground </summary>
    [Export]
    public float groundedAccel;
    /// <summary> The rate at which the player decelerates (slows down) when they are on the ground </summary>
    [Export]
    public float groundedDeceleration;

    /// <summary> The maximum speed the player can travel at on the XZ plane</summary>
    [Export]
    public float maxSpeed;

    /// <summary> The height of the peak of the player's jump, in world space units</summary>
    [Export]
    public float jumpHeight;

    /// <summary> The player's first person camera</summary>
    [Export]
    public Camera3D camera;

    /// <summary> The sensitivity of the player's camera, controlling how fast the camera rotates when the mouse moves</summary>
    [Export]
    public float cameraSensitivity;

    /// <summary> </summary>
    [Export]
    public float sensitivityDivisor;

    /// <summary> The acceleration due to gravity, in world space units per second squared (bigger = fall faster) </summary>
    [Export]
    public float baseGravityMagnitude = 9.8f;

    /// <summary> The scale factor of gravity when the player falls, best to be set above one, as it makes the player's jump feel less floaty</summary>
    [Export]
    public float gravityScaleWhenFalling;

    /// <summary> The amount of the player's velocity (0 - 1) that is removed when the jump button is released early</summary>
    [Export]
    public float cutFactorWhenJumpReleasedEarly;




    private float _pitch, _yaw;

    private RayCast3D _groundedRaycast;

    private bool _grounded;

    // INPUT FLAGS
    private bool _jumpFlag;
    private bool _cutJumpFlag;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        _groundedRaycast = GetNode<RayCast3D>("Grounded Raycast");
        Console.WriteLine("test");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        camera.Basis = Basis.FromEuler(
            new Vector3(Mathf.DegToRad(_pitch), 0, 0)
        );

        if (_groundedRaycast.IsColliding())
        {
            _grounded = true;
        }
        else
        {
            _grounded = false;
        }

        if (Input.IsActionJustPressed("uncapture_mouse"))
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }


        //JUMPING -------------
        if (Input.IsActionJustPressed("jump") && _grounded)
        {
            _jumpFlag = true;
        }

        if (Input.IsActionJustReleased("jump") && LinearVelocity.Y > 0)
        {
            _cutJumpFlag = true;

        }




    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            _pitch -= mouseMotion.Relative.Y * (cameraSensitivity / sensitivityDivisor);
            _yaw -= mouseMotion.Relative.X * (cameraSensitivity / sensitivityDivisor);
        }

    }

    public override void _PhysicsProcess(double delta)
    {

        //MOVEMENT ----------
        Vector2 inputVector = Input.GetVector(
            "move_left",
            "move_right",
            "move_forward",
            "move_back"
        );

        //Prevents players from getting extra speed by providing a diagonal input
        if (inputVector.Length() > 1)
        {
            inputVector = inputVector.Normalized();
        }

        Vector3 desiredVelocity = (new Vector3(inputVector.X, 0, inputVector.Y) * maxSpeed).Rotated(Vector3.Up, Rotation.Y);

        Vector3 velocityError = desiredVelocity - LinearVelocity;
        velocityError = new Vector3(velocityError.X, 0, velocityError.Z);


        if (desiredVelocity == Vector3.Zero)
        {
            ApplyCentralForce(velocityError * groundedDeceleration);
        }
        else
        {

            ApplyCentralForce(velocityError * groundedAccel);
        }


    }

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        Vector3 currentLinearVelocity = state.LinearVelocity;
        Basis = Basis.FromEuler(new Vector3(0, Mathf.DegToRad(_yaw), 0));

        if (currentLinearVelocity.FlattenVector().Length() > maxSpeed)
        {
            LinearVelocity = new Vector3(currentLinearVelocity.Normalized().X * maxSpeed, currentLinearVelocity.Y, currentLinearVelocity.Normalized().Z * maxSpeed);
        }

        if(_jumpFlag){
            currentLinearVelocity.Y = jumpHeight.ToJumpVelocity(baseGravityMagnitude);
            _jumpFlag = false;
        }

        if (_cutJumpFlag)
        {
            currentLinearVelocity.Y *= cutFactorWhenJumpReleasedEarly;
            _cutJumpFlag = false;
        }



        //Handling Gravity Manually
        float appliedGravityMagnitude = currentLinearVelocity.Y < 0 ? baseGravityMagnitude * gravityScaleWhenFalling : baseGravityMagnitude;
        currentLinearVelocity.Y -= appliedGravityMagnitude * (float)state.Step;


        state.LinearVelocity = currentLinearVelocity;

    }

}
