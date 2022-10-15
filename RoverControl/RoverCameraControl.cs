using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverCameraControl : MonoBehaviour
{
    public GameObject cameraman;
    public GameObject rover;
    public bool canMove;

    public void Start() {
        canMove = true;
    }

    public void Update() {
        if (canMove) {
            cameraman.transform.position = rover.transform.position;
            cameraman.transform.rotation = rover.transform.rotation;
        }
    }

    public Vector3[] Scan() {
        List<Vector3> hit = new List<Vector3>();

        return new Vector3[0];
    }
}
