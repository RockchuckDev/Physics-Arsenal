using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInteraction : RayCast3D
{
    /// <summary> The scene that is instantiated and grabs the object. Essentially your "hand"</summary>
    [Export] public PackedScene grabPivotScene;

    /// <summary> The maximum distance away from the camera that the player can interact with objects</summary>
    [Export] public float maxInteractDistance;

    /// <summary> Added to maxInteractDistance to determine how far away a grabbed object needs to be from the player to be grabbed. Adding a small buffer instead of just dropping the object when it's distance from the player exceeds maxInteractDistance gives the player a little wiggle room when grabbing something at the very edge of their interact distance</summary>
    [Export] public float dropDistanceBuffer;

    /// <summary> The strength of the force applied to grabbed objects to move them</summary>
    [Export] public float grabStrength;
    /// <summary> Prevents jitter when moving grabbed objects, but if turned up too high can make it harder to move objects</summary>
    [Export] public float grabDampening;

    /// <summary> The strength of the force applied to grabbed objects to rotate them</summary>
    [Export] public float grabRotationStrength;
    /// <summary> Prevents jitter when rotating grabbed objects, but if turned up too high can make it harder to rotate objects</summary>
    [Export] public float grabRotationDampening;

    /// <summary> The amount to increment/decrement the distance the grabbed object is from the player</summary>
    [Export] public float grabDistanceAdjustmentIncrement;

    /// <summary> How far off to the left/right grabbed objects are when grabbed by the corresponding hands. Done so that when the player grabs two objects, they don't fight for the same space directly in front of the player</summary>
    [Export] public float grabCenterOffset;
    [Export] public RayCast3D playerCrosshair;

    private GrabData rightHandGrabData = new GrabData();
    private GrabData leftHandGrabData = new GrabData();
    private List<GrabData> activeGrabData = new List<GrabData>();


    /// <summary> True if the active grab mode is "Toggle" </summary>
    public bool toggleGrabModeIsOn { get; private set; }

    [Signal]
    public delegate void PlayerChangedGrabModeEventHandler(bool isToggleGrabModeOn);
    private float currentGrabDistance;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TargetPosition = new Vector3(0, 0, -maxInteractDistance);
        EmitSignal(SignalName.PlayerChangedGrabMode, toggleGrabModeIsOn);

        rightHandGrabData.centerOffsetSign = 1;
        leftHandGrabData.centerOffsetSign = -1;

        activeGrabData.Add(rightHandGrabData);
        activeGrabData.Add(leftHandGrabData);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        //GRABBING OBJECT---------------------------
        if (IsColliding())
        {
            Node colliderAsNode = GetCollider() as Node;
            if (colliderAsNode.HasMeta("grabbable"))
            {
                if (Input.IsActionJustPressed("right_grab") && rightHandGrabData.grabbedObject == null)
                {
                    Grab(ref rightHandGrabData, colliderAsNode as RigidBody3D);
                }
                else if (Input.IsActionJustPressed("left_grab") && leftHandGrabData.grabbedObject == null)
                {
                    Grab(ref leftHandGrabData, colliderAsNode as RigidBody3D);
                }
            }
        }



        if (rightHandGrabData.grabbedObject != null || leftHandGrabData.grabbedObject != null)
        {
            if (Input.IsActionJustPressed("decrease_grab_distance"))
            {
                currentGrabDistance -= grabDistanceAdjustmentIncrement;
            }
            else if (Input.IsActionJustPressed("increase_grab_distance"))
            {
                currentGrabDistance += grabDistanceAdjustmentIncrement;
            }
            currentGrabDistance = Mathf.Clamp(currentGrabDistance, 0, maxInteractDistance);
        }
        HandleDrop(ref rightHandGrabData, "right");
        HandleDrop(ref leftHandGrabData, "left");

        if (Input.IsActionJustReleased("toggle_grab_mode"))
        {
            toggleGrabModeIsOn = !toggleGrabModeIsOn;
            EmitSignal(SignalName.PlayerChangedGrabMode, toggleGrabModeIsOn);
        }





        // INTERACTING WITH GRABBED OBJECTS
        if (Input.IsActionJustPressed("right_use"))
        {
            EventBusDeluxe.Fire<PlayerRequestedToInteractWithGrabbedObjectEvent>(new PlayerRequestedToInteractWithGrabbedObjectEvent { requestedHandDataToInteractWithGrabbedObject = rightHandGrabData });
        }

        if (Input.IsActionJustPressed("left_use"))
        {
            EventBusDeluxe.Fire<PlayerRequestedToInteractWithGrabbedObjectEvent>(new PlayerRequestedToInteractWithGrabbedObjectEvent { requestedHandDataToInteractWithGrabbedObject = leftHandGrabData });
        }

    }





    public override void _PhysicsProcess(double delta)
    {
        HandlePositionAndRotation(ref rightHandGrabData);
        HandlePositionAndRotation(ref leftHandGrabData);
    }

    private void HandlePositionAndRotation(ref GrabData grabData)
    {
        if (grabData.grabbedObject != null)
        {
            //POSITION OF GRABBED OBJECT
            Vector3 targetGrabPivotPosition = GlobalPosition + (GlobalTransform.Basis.X * grabData.centerOffsetSign * grabCenterOffset) + (-GlobalTransform.Basis.Z * currentGrabDistance);
            Vector3 toTargetPosition = targetGrabPivotPosition - grabData.grabPivot.GlobalPosition;
            grabData.grabbedObject.ApplyForce(toTargetPosition * grabStrength);
            grabData.grabbedObject.ApplyForce(-grabData.grabbedObject.LinearVelocity * grabDampening);

            //ROTATION OF GRABBED OBJECT
            Basis targetBasis = GlobalBasis * grabData.rotationOffset;


            // HANDLE ROTATION FOR look_at_target METADATA PROPERTY
            if (grabData.grabbedObject.HasMeta("look_at_target"))
            {
                Node targetNode = grabData.grabbedObject.GetNode(grabData.grabbedObject.GetMeta("look_at_target").As<NodePath>());
                Vector3 positionOfTargetNode = Vector3.Zero;
                Vector3 newRotation = Vector3.Zero;
                if (targetNode != null)
                {
                    if (targetNode is RayCast3D targetRaycast)
                    {
                        positionOfTargetNode = targetRaycast.GetCollisionPointOrEndPoint();

                    }
                    else if (targetNode is Node3D targetNode3D)
                    {
                        positionOfTargetNode = targetNode3D.GlobalPosition;
                    }
                    else
                    {
                        GD.PrintErr("look_at_target metadata is not a Node3D!");
                    }

                    // YAW
                    Vector3 pivotToTarget = positionOfTargetNode - grabData.grabPivot.GlobalPosition;
                    // With atan2, the second argument is the "0" axis, the direction you face when your angle is 0. In this case, -Z is forward in Godot, so we need to negate the Z input
                    // In godot, as YAW rotation increases, you rotate counterclockwise on the Y axis. therefore, because the first argument of atan2 is the axis you rotate towards, we need t onegate the x input too
                    // (because if you start on the -z axis and rotate counterclockwise, the next axis you hit is the -X axis)
                    float yawRadians = Mathf.Atan2(-pivotToTarget.X, -pivotToTarget.Z);
                    newRotation.Y = yawRadians;

                    // PITCH
                    float circleRadius = grabData.grabbedObject.GetMeta("look_at_target_y_offset").As<float>();
                    if (circleRadius < 0)
                    {
                        GD.PrintErr("Negative look_at_target_y_offset is not supported.");
                        return;
                    }


                    float hypotenuseLength = pivotToTarget.Length();
                    float clampedAcosInput = Mathf.Clamp(circleRadius / hypotenuseLength, -1.0f, 1.0f);
                    float internalHypotenuseAngle = Mathf.Acos(clampedAcosInput);
                    float lateralDistanceToTarget = new Vector2(pivotToTarget.X, pivotToTarget.Z).Length();
                    float hypotenuseAngleFromZero = Mathf.Atan2(pivotToTarget.Y, lateralDistanceToTarget);
                    float pitchRadians = hypotenuseAngleFromZero + internalHypotenuseAngle;
                    newRotation.X = pitchRadians - Mathf.Pi/2;
                    targetBasis = Basis.FromEuler(newRotation);

                }
                else
                {
                    GD.PrintErr("look_at_target node not found");
                    return;
                }

            }

            Quaternion toTargetBasis = (targetBasis * grabData.grabPivot.GlobalBasis.Inverse()).GetRotationQuaternion();
            grabData.grabPivot.ApplyTorque(toTargetBasis.GetAxis() * toTargetBasis.GetAngle() * grabRotationStrength);
            grabData.grabPivot.ApplyTorque(-grabData.grabPivot.AngularVelocity * grabRotationDampening);
        }
    }

    private void Grab(ref GrabData grabData, RigidBody3D objectToGrab)
    {
        grabData.grabbedObject = objectToGrab;



        Vector3 targetGrabPosition = GetCollisionPoint();
        if (grabData.grabbedObject.FindChild("Grab Point") != null)
        {
            targetGrabPosition = (grabData.grabbedObject.FindChild("Grab Point") as Node3D).GlobalPosition;
        }

        grabData.grabPivot = grabPivotScene.Instantiate<RigidBody3D>();

        playerCrosshair.AddException(grabData.grabbedObject);


        if (grabData.grabbedObject.HasMeta("look_at_target"))
        {
            grabData.grabbedObject.AddChild(grabData.grabPivot);
            grabData.grabPivot.GlobalPosition = targetGrabPosition;
            grabData.grabPivot.Rotation = Vector3.Zero;
            Transform3D globalGrabPivotTransform = grabData.grabPivot.GlobalTransform;
            grabData.grabbedObject.RemoveChild(grabData.grabPivot);
            GetTree().Root.AddChild(grabData.grabPivot);
            grabData.grabPivot.GlobalTransform = globalGrabPivotTransform;
        }else{
            GetTree().Root.AddChild(grabData.grabPivot);
            grabData.grabPivot.GlobalPosition = targetGrabPosition;

        }



        currentGrabDistance = GlobalPosition.DistanceTo(targetGrabPosition);

        if (grabData.grabbedObject.HasMeta("rotation_override"))
        {
            Vector3 rotationOverride = grabData.grabbedObject.GetMeta("rotation_override").AsVector3();
            Vector3 rotationOverrideInRadians = new Vector3(Mathf.DegToRad(rotationOverride.X), Mathf.DegToRad(rotationOverride.Y), Mathf.DegToRad(rotationOverride.Z));
            grabData.rotationOffset = Basis.FromEuler(rotationOverrideInRadians) * grabData.grabbedObject.GlobalBasis.Inverse();
        }
        else
        {
            grabData.rotationOffset = grabData.grabPivot.GlobalBasis * GlobalBasis.Inverse();
        }

        grabData.fixedJoint = new Generic6DofJoint3D();
        grabData.fixedJoint.Position = GetCollisionPoint();
        GetTree().Root.AddChild(grabData.fixedJoint);
        grabData.fixedJoint.NodeA = grabData.grabPivot.GetPath();
        grabData.fixedJoint.NodeB = grabData.grabbedObject.GetPath();

        grabData.grabbedObject.FindChildOfType<IsGrabbedTracker>()?.UpdateGrabTracker(true);
    }

    private void HandleDrop(ref GrabData grabData, string handName)
    {
        if (grabData.grabbedObject != null)
        {
            Vector3 toGrabbedObject = grabData.grabbedObject.GlobalPosition - GlobalPosition;
            //Dot product is some straight up magic, even my calculus teacher doesn't know why it works.
            // All you need to know, is that if the dot product of two vectors is positive, they are less than 90 degrees apart, and if the product is negative, they are more than 90 degrees apart
            // What this means for us, is that if the dot product of the direction the player is facing and the direction the object is from this Node is negative, the object is behind the player
            if (toGrabbedObject.Dot(-GlobalBasis.Z) < 0 || toGrabbedObject.Length() > maxInteractDistance + dropDistanceBuffer)
            {
                Drop(ref grabData);
            }
            else if (Input.IsActionJustReleased(handName + "_grab"))
            {
                if (toggleGrabModeIsOn && !grabData.readyToDrop)
                {
                    grabData.readyToDrop = true;
                }
                else
                {
                    Drop(ref grabData);
                }

            }

        }



    }

    private void Drop(ref GrabData grabData)
    {
        playerCrosshair.RemoveException(grabData.grabbedObject);
        if (grabData.grabbedObject == null)
        {
            GD.Print("grabData.grabbedObject is null");
            return;
        }
        grabData.grabbedObject.FindChildOfType<IsGrabbedTracker>()?.UpdateGrabTracker(false);
        grabData.grabPivot.QueueFree();
        grabData.fixedJoint.QueueFree();
        grabData.grabPivot = null;
        grabData.grabbedObject = null;
        grabData.readyToDrop = false;

    }



}

public class GrabData
{
    /// <summary> The active grab pivot </summary>
    public RigidBody3D grabPivot;
    /// <summary> The active fixed joint </summary>
    public Generic6DofJoint3D fixedJoint;
    /// <summary> The currently grabbed object </summary>
    public RigidBody3D grabbedObject;
    /// <summary> The rotation difference between the grabbed object when it is first grabbed and the camera
    public Basis rotationOffset;
    /// <summary> Helps decide what press of the grab button causes the player to drop the object when in "toggle" grab mode </summary>
    public bool readyToDrop;
    /// <summary> Negative means this hand will hold objects a little off center to the left, positive means off to the right </summary>
    public float centerOffsetSign;

}
