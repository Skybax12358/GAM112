using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CarController : MonoBehaviour
{

    //Public variables

    //Wheel variables
    public float idealRPM = 500f;
    public float maxRPM = 1000f;

    public Transform centerOfGravity;
    public Rigidbody rb;

    public WheelCollider wheelFR;
    public WheelCollider wheelFL;
    public WheelCollider wheelRR;
    public WheelCollider wheelRL;

    //Suspension + braking variables
    public float torque = 25f;
    public float brakeTorque = 100f;

    public float AntiRoll = 20000.0f;

    //Drivetype variables
    public enum DriveMode { Front, Rear, All };
    public DriveMode driveMode = DriveMode.Rear;

    //UI variables
    public Text speedText;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfGravity.localPosition;
    }

    //Get speed based on wheel size
    public float Speed() {
        return wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 1000f;
    }

    //Returns RPM for use with throttle
    public float Rpm() {
        return wheelRL.rpm;
    }

    void FixedUpdate() {

        if (speedText != null)
            speedText.text = "Speed: " + Speed().ToString("f0") + " km/h";

        //Debug.Log ("Speed: " + (wheelRR.radius * Mathf.PI * wheelRR.rpm * 60f / 1000f) + "km/h    RPM: " + wheelRL.rpm);

        //Smooth acceleration * twisting power of the car
        float scaledTorque = Input.GetAxis("Vertical") * torque;

        //Scale torque to be higher down low, clamps overall torque
        if (wheelRL.rpm < idealRPM)
            scaledTorque = Mathf.Lerp(scaledTorque / 10f, scaledTorque, wheelRL.rpm / idealRPM);
        else
            scaledTorque = Mathf.Lerp(scaledTorque, 0, (wheelRL.rpm - idealRPM) / (maxRPM - idealRPM));

        //Suspension + anti roll
        DoRollBar(wheelFR, wheelFL);
        DoRollBar(wheelRR, wheelRL);

        //Drivemode - RWD, FWD or ALL
        wheelFR.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque;
        wheelFL.motorTorque = driveMode == DriveMode.Rear ? 0 : scaledTorque;
        wheelRR.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque;
        wheelRL.motorTorque = driveMode == DriveMode.Front ? 0 : scaledTorque;

        //Movement
        if (Input.GetButton("Fire1")) {
            wheelFR.brakeTorque = brakeTorque;
            wheelFL.brakeTorque = brakeTorque;
            wheelRR.brakeTorque = brakeTorque;
            wheelRL.brakeTorque = brakeTorque;
        }
        else {
            wheelFR.brakeTorque = 0;
            wheelFL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
            wheelRL.brakeTorque = 0;
        }
    }

    //Antiroll function
    void DoRollBar(WheelCollider WheelL, WheelCollider WheelR) {
        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;

        //if either wheel is feeling force
        bool groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;

        bool groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;

        float antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
            rb.AddForceAtPosition(WheelL.transform.up * -antiRollForce,
                                         WheelL.transform.position);
        if (groundedR)
            rb.AddForceAtPosition(WheelR.transform.up * antiRollForce,
                                         WheelR.transform.position);
    }

}
