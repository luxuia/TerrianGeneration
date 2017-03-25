using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookTarget : MonoBehaviour {

    public Transform Target;
    public float Dist;

    public Vector3 LookDir = new Vector3(1, -1, 1);

	// Update is called once per frame
	void Update () {
        if (Target != null)
        {
            transform.position = Target.position - LookDir.normalized * Dist;
            transform.LookAt(Target);
        }
	}
}
