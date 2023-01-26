using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    float rollControlSensitivity = 0.2f;
    [SerializeField]
    float pitchControlSensitivity = 0.2f;
    [SerializeField]
    float yawControlSensitivity = 0.2f;

    [Range(-1, 1)]
    public float Pitch;
    [Range(-1, 1)]
    public float Yaw;
    [Range(-1, 1)]
    public float Roll;
    [Range(0, 1)]
    public float Flap;
    [SerializeField]
    Text displayText = null;

    [Header("Tail Configs")]
    [SerializeField]
    AeroSurfaceConfig BrakingTailConfig = null;
    [SerializeField]
    AeroSurfaceConfig DivingTailConfig = null;
    [SerializeField]
    AeroSurfaceConfig NormalTailConfig = null;

    [Header("Wing Configs")]
    [SerializeField]
    AeroSurfaceConfig Wing1UpsideDownConfig = null;
    [SerializeField]
    AeroSurfaceConfig Wing2UpsideDownConfig = null;
    [SerializeField]
    AeroSurfaceConfig Wing3UpsideDownConfig = null;
    [SerializeField]
    AeroSurfaceConfig Wing1NormalConfig = null;
    [SerializeField]
    AeroSurfaceConfig Wing2NormalConfig = null;
    [SerializeField]
    AeroSurfaceConfig Wing3NormalConfig = null;

    [Header("Wing Surfaces")]
    [SerializeField]
    AeroSurface LeftWing1Surface;
    [SerializeField]
    AeroSurface LeftWing2Surface;
    [SerializeField]
    AeroSurface LeftWing3Surface;
    [SerializeField]
    AeroSurface RightWing1Surface;
    [SerializeField]
    AeroSurface RightWing2Surface;
    [SerializeField]
    AeroSurface RightWing3Surface;

    [Header("Tweakable Settings")]
    [SerializeField]
    float upsideDownThreshold;
    [SerializeField]
    float diveTailAngle;

    [Header("Thurst Settings")]
    public float thrustRotationSpeed;
    public float targetThrustRotation;
    public float thrustRotation;
    bool thrustEnabled;
    
    float brakesTorque;

    [Header("Flap Settings")]
    public float targetAngle;
    public float turnSpeed = 0.05f;
    public Transform rightUpTargetTransform;
    public Transform leftUpTargetTransform;
    public Transform rightDownTargetTransform;
    public Transform leftDownTargetTransform;
    Quaternion rightRotGoal;
    Quaternion leftRotGoal;
    Vector3 rightDirection;
    Vector3 leftDirection;

    // flip flops
    bool breakFlipFlop = false;
    bool diveFlipFlop = false;
    bool upFlapFlipFlop = false;
    bool downFlapFlipFlop = false;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    GameObject Wings;
    GameObject RightWing;
    GameObject LeftWing;
    GameObject Tail;
    GameObject Body;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        Tail = transform.GetChild(0).gameObject.transform.Find("Tail").gameObject;
        Wings = transform.GetChild(0).gameObject.transform.Find("Wings").gameObject;
        RightWing = Wings.transform.GetChild(0).gameObject;
        LeftWing = Wings.transform.GetChild(1).gameObject;
        Body = transform.GetChild(0).gameObject.transform.Find("Body").gameObject;

        thrustRotation = 0.65f;
    }

    private void Update()
    {
        if (transform.up.y < upsideDownThreshold) 
        {
            LeftWing1Surface.config = Wing1UpsideDownConfig;
            LeftWing2Surface.config = Wing2UpsideDownConfig;
            LeftWing3Surface.config = Wing3UpsideDownConfig;
            RightWing1Surface.config = Wing1UpsideDownConfig;
            RightWing2Surface.config = Wing2UpsideDownConfig;
            RightWing3Surface.config = Wing3UpsideDownConfig;
        }
        else
        {
            LeftWing1Surface.config = Wing1NormalConfig;
            LeftWing2Surface.config = Wing2NormalConfig;
            LeftWing3Surface.config = Wing3NormalConfig;
            RightWing1Surface.config = Wing1NormalConfig;
            RightWing2Surface.config = Wing2NormalConfig;
            RightWing3Surface.config = Wing3NormalConfig;
        }

        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            thrustEnabled = !thrustEnabled;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // thrustForwardDirectionMask =-1;
            // thrustUpDirectionMask = 0;

            if (!breakFlipFlop) 
            {
                Wings.transform.Rotate(new Vector3(-30, 0, 0));
                RightWing.transform.Rotate(new Vector3(0, 0, 25));
                LeftWing.transform.Rotate(new Vector3(0, 0, -25));
                Tail.transform.Rotate(new Vector3(30, 0, 0));
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = BrakingTailConfig;
            } 
            else 
            {
                Wings.transform.Rotate(new Vector3(30, 0, 0));
                RightWing.transform.Rotate(new Vector3(0, 0, -25));
                LeftWing.transform.Rotate(new Vector3(0, 0, 25));
                Tail.transform.Rotate(new Vector3(-30, 0, 0));
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = NormalTailConfig;
            }
            breakFlipFlop = !breakFlipFlop;
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) 
        {
            if (!diveFlipFlop)
            {
                RightWing.transform.localEulerAngles = new Vector3(90, 60, 0);
                LeftWing.transform.localEulerAngles = new Vector3(90, -60, 180);
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = DivingTailConfig;
                Tail.transform.Rotate(diveTailAngle, 0, 0);
                Vector3 pos = Wings.transform.localPosition;
                pos.z += 0.5f;
                Wings.transform.localPosition = pos;
            }
            else 
            {
                RightWing.transform.localEulerAngles = new Vector3(0, 0, 0);
                LeftWing.transform.localEulerAngles = new Vector3(0, 0, 180);
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = NormalTailConfig;
                Tail.transform.Rotate(-diveTailAngle, 0, 0);
                Vector3 pos = Wings.transform.localPosition;
                pos.z -= 0.5f;
                Wings.transform.localPosition = pos;
            }
            diveFlipFlop = !diveFlipFlop;
        }

        // up flap
        if (Input.GetKeyDown(KeyCode.Alpha5)) 
        {
            downFlapFlipFlop = false;
            upFlapFlipFlop = !upFlapFlipFlop;
        }

        // down flap
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            upFlapFlipFlop = false;
            downFlapFlipFlop = !downFlapFlipFlop;
        }

        if (upFlapFlipFlop)
        {
            RightWing.transform.rotation = Quaternion.Slerp(RightWing.transform.rotation, rightUpTargetTransform.rotation, turnSpeed);

            LeftWing.transform.rotation = Quaternion.Slerp(LeftWing.transform.rotation, leftUpTargetTransform.rotation, turnSpeed);
        }
        else if (downFlapFlipFlop)
        {
            RightWing.transform.rotation = Quaternion.Slerp(RightWing.transform.rotation, rightDownTargetTransform.rotation, turnSpeed);

            LeftWing.transform.rotation = Quaternion.Slerp(LeftWing.transform.rotation, leftDownTargetTransform.rotation, turnSpeed);
        }

        if (rb.velocity.magnitude < 1)
        {
            targetThrustRotation = 0.65f;
        }
        else
        {
            targetThrustRotation = 0.15f;
        }

        thrustRotation = Mathf.SmoothStep(thrustRotation, targetThrustRotation, thrustRotationSpeed);

        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(aircraftPhysics.thrustPercent * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustEnabled(thrustEnabled);
        aircraftPhysics.SetThrustDirection(thrustRotation * transform.up + ((1 - thrustRotation) * transform.forward));
    }

    public void SetControlSurfecesAngles(float pitch, float roll, float yaw, float flap)
    {
        foreach (var surface in controlSurfaces)
        {
            if (surface == null || !surface.IsControlSurface) continue;
            switch (surface.InputType)
            {
                case ControlInputType.Pitch:
                    surface.SetFlapAngle(pitch * pitchControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Roll:
                    surface.SetFlapAngle(roll * rollControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Yaw:
                    surface.SetFlapAngle(yaw * yawControlSensitivity * surface.InputMultiplyer);
                    break;
                case ControlInputType.Flap:
                    surface.SetFlapAngle(Flap * surface.InputMultiplyer);
                    break;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
    }
}
