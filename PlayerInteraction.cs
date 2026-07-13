using Godot;
using System;

public partial class PlayerInteraction : RayCast3D
{
    //grabPivotScene is nothing more than a Rigidbody3D with a mass of 1, that's it
    [Export] public PackedScene grabPivotScene;
    //Called InteractDistance because soon this package will have the ability to interact with other objects, such as talking to an NPC
    [Export] public float maxInteractDistance;
    [Export] public float grabStrength;
    //Prevents jitter, but if turned up to high can make it harder to move objects
    [Export] public float grabDampening;
    [Export] public float grabRotationStrength;
    [Export] public float grabRotationDampening;
    [Export] public float grabDistanceAdjustmentIncrement;
    [Export] public float grabCenterOffset;

    private Basis rightHandRotationOffset;
    private Basis leftHandRotationOffset;
    private RigidBody3D activeRightGrabPivot;
    private RigidBody3D activeLeftGrabPivot;
    private Generic6DofJoint3D activeRightHandFixedJoint;
    private Generic6DofJoint3D activeLeftHandFixedJoint;
    private RigidBody3D activeRightGrabObject;
    private RigidBody3D activeLeftGrabObject;
    private float currentGrabDistance;
    private Vector3 targetRightHandPosition;
    private Vector3 targetLeftHandPosition;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TargetPosition = new Vector3(0, 0, -maxInteractDistance);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        //GRABBING OBJECT---------------------------
        if (IsColliding())
        {
            Node colliderAsNode = GetCollider() as Node;
            if (colliderAsNode.GetMeta("grabbable", false).AsBool())
            {

                //GRABBING OBJECT WITH RIGHT HAND---------------------------
                if (Input.IsActionJustPressed("right_grab") && activeRightGrabPivot == null)
                {
                    activeRightGrabObject = colliderAsNode as RigidBody3D;


                    activeRightGrabPivot = grabPivotScene.Instantiate<RigidBody3D>();
                    activeRightGrabPivot.Position = GetCollisionPoint();
                    GetTree().Root.AddChild(activeRightGrabPivot);



                    //Rotation is applied right to left, so here we undo the rotation of this Node, then apply the rotation of the activeRightGrabPivot, telling us the rotation difference between this Node and the activeRightGrabPivot
                    rightHandRotationOffset = activeRightGrabPivot.GlobalBasis * GlobalBasis.Inverse();

                    activeRightHandFixedJoint = new Generic6DofJoint3D();
                    activeRightHandFixedJoint.Position = GetCollisionPoint();
                    GetTree().Root.AddChild(activeRightHandFixedJoint);
                    activeRightHandFixedJoint.NodeA = activeRightGrabPivot.GetPath();
                    activeRightHandFixedJoint.NodeB = activeRightGrabObject.GetPath();


                }
                else if (Input.IsActionJustPressed("left_grab") && activeLeftGrabPivot == null)
                {
                    activeLeftGrabObject = colliderAsNode as RigidBody3D;

                    activeLeftGrabPivot = grabPivotScene.Instantiate<RigidBody3D>();
                    activeLeftGrabPivot.Position = GetCollisionPoint();
                    GetTree().Root.AddChild(activeLeftGrabPivot);

                    currentGrabDistance = GlobalPosition.DistanceTo(GetCollisionPoint());

                    leftHandRotationOffset = activeLeftGrabPivot.GlobalBasis * GlobalBasis.Inverse();

                    activeLeftHandFixedJoint = new Generic6DofJoint3D();
                    activeLeftHandFixedJoint.Position = GetCollisionPoint();
                    GetTree().Root.AddChild(activeLeftHandFixedJoint);
                    activeLeftHandFixedJoint.NodeA = activeLeftGrabPivot.GetPath();
                    activeLeftHandFixedJoint.NodeB = activeLeftGrabObject.GetPath();


                }
            }

        }

        if (activeRightGrabPivot != null || activeLeftGrabPivot != null)
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

            //DROPPING OBJECT---------------------------
            if (activeRightGrabPivot != null)
            {

                //DROPPING OBJECT
                Vector3 toGrabbedObject = activeRightGrabObject.GlobalPosition - GlobalPosition;
                //Dot product is some straight up magic, even my calculus teacher doesn't know why it works.
                // All you need to know, is that if the dot product of two vectors is positive, they are less than 90 degrees apart, and if the product is negative, they are more than 90 degrees apart
                // What this means for us, is that if the dot product of the direction the player is facing and the direction the object is from this Node is negative, the object is behind the player
                if (Input.IsActionJustReleased("right_grab") || toGrabbedObject.Dot(-GlobalBasis.Z) < 0)
                {
                    DropRight();
                }

            }
            if (activeLeftGrabPivot != null)
            {
                Vector3 toGrabbedObject = activeLeftGrabObject.GlobalPosition - GlobalPosition;
                if (Input.IsActionJustReleased("left_grab") || toGrabbedObject.Dot(-GlobalBasis.Z) < 0)
                {
                    DropLeft();
                }

            }
        }


    }



    public override void _PhysicsProcess(double delta)
    {
        if (activeRightGrabPivot != null)
        {
            //POSITION OF GRABBED OBJECT
            targetRightHandPosition = GlobalPosition + GlobalTransform.Basis.X * grabCenterOffset + -GlobalTransform.Basis.Z * currentGrabDistance;
            Vector3 toTargetPosition = targetRightHandPosition - activeRightGrabPivot.Position;
            activeRightGrabPivot.ApplyForce(toTargetPosition * grabStrength);
            activeRightGrabPivot.ApplyForce(-activeRightGrabPivot.LinearVelocity * grabDampening);

            //ROTATION OF GRABBED OBJECT
            Basis targetBasis = GlobalBasis * rightHandRotationOffset;
            Quaternion toTargetBasis = (targetBasis * activeRightGrabPivot.GlobalBasis.Inverse()).GetRotationQuaternion();
            activeRightGrabPivot.ApplyTorque(toTargetBasis.GetAxis() * toTargetBasis.GetAngle() * grabRotationStrength);
            activeRightGrabPivot.ApplyTorque(-activeRightGrabPivot.AngularVelocity * grabRotationDampening);
        }

        if (activeLeftGrabPivot != null)
        {
            //POSITION OF GRABBED OBJECT
            targetLeftHandPosition = GlobalPosition - GlobalTransform.Basis.X * grabCenterOffset + -GlobalTransform.Basis.Z * currentGrabDistance;
            Vector3 toTargetPosition = targetLeftHandPosition - activeLeftGrabPivot.Position;
            activeLeftGrabPivot.ApplyForce(toTargetPosition * grabStrength);
            activeLeftGrabPivot.ApplyForce(-activeLeftGrabPivot.LinearVelocity * grabDampening);

            //ROTATION OF GRABBED OBJECT
            Basis targetBasis = GlobalBasis * leftHandRotationOffset;
            Quaternion toTargetBasis = (targetBasis * activeLeftGrabPivot.GlobalBasis.Inverse()).GetRotationQuaternion();
            activeLeftGrabPivot.ApplyTorque(toTargetBasis.GetAxis() * toTargetBasis.GetAngle() * grabRotationStrength);
            activeLeftGrabPivot.ApplyTorque(-activeLeftGrabPivot.AngularVelocity * grabRotationDampening);
        }
    }


    public void DropRight()
    {
        activeRightGrabPivot.QueueFree();
        activeRightHandFixedJoint.QueueFree();
        activeRightGrabPivot = null;
    }

    private void DropLeft()
    {
        activeLeftGrabPivot.QueueFree();
        activeLeftHandFixedJoint.QueueFree();
        activeLeftGrabPivot = null;
    }
}
