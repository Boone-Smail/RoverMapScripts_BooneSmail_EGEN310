using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate_Camera : MonoBehaviour
{
    public GameObject cameraPivot;
    public float rotateConst;

    public void Update() {
        cameraPivot.transform.Rotate(0,rotateConst,0);
    }
}
