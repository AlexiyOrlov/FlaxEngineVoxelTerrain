using FlaxEngine;

public class FreeCamera : Script
{
    [Limit(0, 100), Tooltip("Camera movement speed factor")]
    public float MoveSpeed { get; set; } = 4;

    [Tooltip("Camera rotation smoothing factor")]
    public float CameraSmoothing { get; set; } = 20.0f;

    private float pitch;
    private float yaw;

    public override void OnStart()
    {
        var initialEulerAngles = Actor.Orientation.EulerAngles;
        pitch = initialEulerAngles.X;
        yaw = initialEulerAngles.Y;
    }

    public override void OnUpdate()
    {
        Screen.CursorVisible = false;
        Screen.CursorLock = CursorLockMode.Locked;

        var mouseDelta = new Float2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        pitch = Mathf.Clamp(pitch + mouseDelta.Y, -88, 88);
        yaw += mouseDelta.X;
    }

    public override void OnFixedUpdate()
    {
        var inputH = Input.GetAxis("Horizontal");
        var inputV = Input.GetAxis("Vertical");
        var jump=Input.GetAxis("VerticalMove");
        
        Vector3 move= new Vector3(inputH*MoveSpeed,-jump*MoveSpeed/2, inputV*MoveSpeed);
        var actorTransform=Actor.Transform;
        var camFactor = Mathf.Saturate(CameraSmoothing * Time.DeltaTime);
        actorTransform.Orientation = Quaternion.Lerp(actorTransform.Orientation, Quaternion.Euler(pitch, yaw, 0), camFactor);
        var vecTransformed=actorTransform.TransformDirection(move);
        actorTransform.Translation+=vecTransformed;
        Actor.Transform=actorTransform;
    }
}
