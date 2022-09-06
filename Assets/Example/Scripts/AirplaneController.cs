using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AirplaneController : MonoBehaviour
{
    [SerializeField]
    List<AeroSurface> controlSurfaces = null;
    [SerializeField]
    List<WheelCollider> wheels = null;
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
    [SerializeField]
    AeroSurfaceConfig BrakingTailConfig = null;
    [SerializeField]
    AeroSurfaceConfig NormalTailConfig = null;

    float thrustPercent;
    int thrustForwardDirectionMask;
    int thrustUpDirectionMask;
    float brakesTorque;

    AircraftPhysics aircraftPhysics;
    Rigidbody rb;

    GameObject Wings;
    GameObject Tail;

    private void Start()
    {
        aircraftPhysics = GetComponent<AircraftPhysics>();
        rb = GetComponent<Rigidbody>();

        thrustForwardDirectionMask = 1;
        thrustUpDirectionMask = 0;

        Tail = transform.GetChild(0).gameObject.transform.Find("Tail").gameObject;
        Wings = transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;
    }

    private void Update()
    {
        Pitch = Input.GetAxis("Vertical");
        Roll = Input.GetAxis("Horizontal");
        Yaw = Input.GetAxis("Yaw");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            thrustPercent = thrustPercent > 0 ? 0 : 1f;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flap = Flap > 0 ? 0 : 0.3f;
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            brakesTorque = brakesTorque > 0 ? 0 : 100f;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            thrustForwardDirectionMask = 1;
            thrustUpDirectionMask = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            thrustForwardDirectionMask = 1;
            thrustUpDirectionMask = 0;
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // thrustForwardDirectionMask =-1;
            // thrustUpDirectionMask = 0;

            if (Wings.transform.localEulerAngles.magnitude < 0.1) 
            {
                Wings.transform.Rotate(new Vector3(-30, 0, 0));
                Wings.transform.GetChild(0).transform.Rotate(new Vector3(0, 0, 25));
                Wings.transform.GetChild(1).transform.Rotate(new Vector3(0, 0, -25));
                Tail.transform.Rotate(new Vector3(30, 0, 0));
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = BrakingTailConfig;
            } 
            else 
            {
                Wings.transform.Rotate(new Vector3(30, 0, 0));
                Wings.transform.GetChild(0).transform.Rotate(new Vector3(0, 0, -25));
                Wings.transform.GetChild(1).transform.Rotate(new Vector3(0, 0, 25));
                Tail.transform.Rotate(new Vector3(-30, 0, 0));
                Tail.transform.GetChild(0).GetComponent<AeroSurface>().config = NormalTailConfig;
            }
        }

        displayText.text = "V: " + ((int)rb.velocity.magnitude).ToString("D3") + " m/s\n";
        displayText.text += "A: " + ((int)transform.position.y).ToString("D4") + " m\n";
        displayText.text += "T: " + (int)(thrustPercent * 100) + "%\n";
        displayText.text += brakesTorque > 0 ? "B: ON" : "B: OFF";
    }

    private void FixedUpdate()
    {
        SetControlSurfecesAngles(Pitch, Roll, Yaw, Flap);
        aircraftPhysics.SetThrustPercent(thrustPercent);
        aircraftPhysics.SetThrustDirection(transform.up * thrustUpDirectionMask + transform.forward * thrustForwardDirectionMask);
        foreach (var wheel in wheels)
        {
            wheel.brakeTorque = brakesTorque;
            // small torque to wake up wheel collider
            wheel.motorTorque = 0.01f;
        }
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
