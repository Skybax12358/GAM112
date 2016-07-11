using UnityEngine;
using System.Collections;

public class WheelSpin : MonoBehaviour {

    //Public variables
    public WheelCollider wheelCollider;

	void Start () {
	
	}
	
    //update height of the cylinder to match wheel
	void FixedUpdate () {
        UpdateWheelHeight(this.transform, wheelCollider);
	}

    //UpdateWheelHeight
    void UpdateWheelHeight(Transform wheelTransform, WheelCollider collider) {

        Vector3 localPosition = wheelTransform.localPosition;

        WheelHit hit = new WheelHit();

        //if wheel collider hits the ground
        if (collider.GetGroundHit(out hit)) { //change wheel position to match collider
            float hitY = collider.transform.InverseTransformPoint(hit.point).y;

            localPosition.y = hitY + collider.radius;

        }

        //else stay at default position
        else {
            localPosition = Vector3.Lerp(localPosition, -Vector3.up * collider.suspensionDistance, .05f);
        }

        //Rotate wheel
        wheelTransform.localPosition = localPosition;

        wheelTransform.localRotation = Quaternion.Euler(0, collider.steerAngle, 90);
    }
}
