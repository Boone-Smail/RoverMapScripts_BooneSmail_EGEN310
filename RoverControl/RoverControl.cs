// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverControl : MonoBehaviour
{
    public GameObject rover;
    public GameObject roverSensor;
    public GameObject simRover;
    public GameObject marchable;
    public GameObject marchedParent;
    public GameObject firstCamera;
    public GameObject secondCamera;
    public float speedConst;
    public float rotateConst;
    public float scanConst;
    public Vector3 buildCubeMarchAt;
    private bool canMove;
    private bool canScan;
    private bool canSwapDisplay;
    private int displayNum;

    private CubeSet collection;

    void Start() {
        canMove = true;
        canScan = true;
        collection = new CubeSet(31);
        canSwapDisplay = true;
        displayNum = 0;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.W) && canMove) {
            rover.transform.position += rover.transform.forward * speedConst;
        }
        if (Input.GetKey(KeyCode.S) && canMove) {
            rover.transform.position -= rover.transform.forward * speedConst;
        }
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && canMove) {
            rover.transform.Rotate(new Vector3(0,-rotateConst,0));
        }
        if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && canMove) {
            rover.transform.Rotate(new Vector3(0,rotateConst,0));
        }
        if (Input.GetKey(KeyCode.Space) && canScan) {
            canMove = false;
            canScan = false;
            StartCoroutine(Scan());
        }
        if (Input.GetKey(KeyCode.T) && canSwapDisplay) {
            Debug.Log("Swapping stage 1...");
            canSwapDisplay = false;
            StartCoroutine(SwapDisplay());
        }
        if (Input.GetKey(KeyCode.G)) {
            Debug.Log("Moving rover sim to match rover in realspace...");
            simRover.transform.position = buildCubeMarchAt + rover.transform.position + new Vector3(0,0.5f,0);
            simRover.transform.rotation = rover.transform.rotation;
        }
    }

    public IEnumerator SwapDisplay() {
        displayNum++;
        if (displayNum > 1) {
            displayNum = 0;
        }
        Debug.Log("\tSwapping stage 2..." + "\n\t\tDisplay num: " + displayNum);

        if (displayNum == 1) {
            secondCamera.SetActive(true);
            firstCamera.SetActive(false);
        }
        else {
            firstCamera.SetActive(true);
            secondCamera.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
        canSwapDisplay = true;
    }

    public IEnumerator Scan() {
        
        // Instantiating needs
        Debug.Log("Beginning scan...");
        Vector3 aim;
        RaycastHit hit;
        List<Vector3> tagged = new List<Vector3>();

        // Getting directions based on scan
        Vector3 Horizontal = Vector3.Cross(roverSensor.transform.right, roverSensor.transform.up);
        // Debug.Log(Horizontal.ToString());
        Vector3 Leap = Vector3.Cross(-roverSensor.transform.forward, roverSensor.transform.up);

        // Creating Layer Mask
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        
        // Outward Scan
        for (float i = -2.5f; i <= 2.5f; i+= 0.5f) {
            for (float j = -4.5f; j <= 4.5f; j+=0.5f) {
                aim = rover.transform.forward * 10;
                aim += new Vector3(0,i,0) + (j*Horizontal);
                Debug.DrawRay(roverSensor.transform.position, aim, Color.red, scanConst);
                //Debug.Log(Physics.Raycast(roverSensor.transform.position, aim, out hit, scanConst, 0));
                if (Physics.Raycast(roverSensor.transform.position, aim, out hit, 10f, layerMask)) {
                    tagged.Add(aim * (hit.distance / aim.magnitude));
                }
                yield return new WaitForSeconds(scanConst);
            }
        }

        // Floor Scan
        for (float i = -3.5f; i <= 3.5f; i+= 0.5f) {
            for (float j = 0.5f; j <= 9.5; j+= 0.5f) {
                aim = roverSensor.transform.forward;
                aim += new Vector3(0,-1.5f,0) + (Horizontal * i) + (Leap * j);
                Debug.DrawRay(roverSensor.transform.position, aim, Color.red, scanConst);
                //Debug.Log(Physics.Raycast(roverSensor.transform.position, aim, out hit, scanConst, 0));
                if (Physics.Raycast(roverSensor.transform.position, aim, out hit, 10f, layerMask)) {
                    tagged.Add(aim * (hit.distance / aim.magnitude));
                    //Debug.Log("Floor hit!");
                }
                yield return new WaitForSeconds(scanConst);
            }
        }
        // Vector3[] fromFloor = scanFloor();
        // for (int i = 0; i < fromFloor.Length; i++) {
        //     tagged.Add(fromFloor[i]);
        //     Debug.Log("Added point from floor: " + fromFloor[i].ToString());
        // }
        simRover.transform.position = buildCubeMarchAt + rover.transform.position + new Vector3(0,0.5f,0);
        simRover.transform.rotation = rover.transform.rotation;
        canMove = true;
        canScan = true;
        march(Appropriate(tagged.ToArray()));
    }

    public Vector3[] scanFloor() {
        Vector3 aim;
        RaycastHit hit;
        List<Vector3> tagged = new List<Vector3>();
        int layerMask = 1 << 8;

        for (float i = -2.5f; i <= 2.5f; i+= 0.5f) {
            for (float j = 0f; j <= 4; j+= 0.5f) {
                aim = roverSensor.transform.forward;
                aim += new Vector3(i,-1.5f,j);
                Debug.DrawRay(roverSensor.transform.position, aim, Color.red, scanConst);
                //Debug.Log(Physics.Raycast(roverSensor.transform.position, aim, out hit, scanConst, 0));
                if (Physics.Raycast(roverSensor.transform.position, aim, out hit, 10f, layerMask)) {
                    tagged.Add(aim * (hit.distance / aim.magnitude));
                    Debug.Log("Floor hit!");
                }
            }
        }

        return tagged.ToArray();
    }

    public PointSet Appropriate(Vector3[] o) {
        Debug.Log("Appropriating...");
        PointSet temp = new PointSet(11);
        Vector3 spot;
        foreach (Vector3 i in o) {
            spot = i + roverSensor.transform.position;
            spot.x = Mathf.Round(2*spot.x)/2f;
            spot.y = Mathf.Round(2*spot.y)/2f;
            spot.z = Mathf.Round(2*spot.z)/2f;
            //Debug.Log(temp.addPoint(spot));
            if (temp.addPoint(spot)) {
                //Debug.Log(spot.ToString());
            }
        }
        return temp;
    }

    public void march(PointSet contained) {
        
        // List<Vector3> truePoints = new List<Vector3>();
        // foreach (Vector3 i in contained.getPoints()) {
        //     truePoints.Add(i + roverSensor.transform.position);
        // }

        collection.march(contained.getPoints(), marchable, buildCubeMarchAt, marchedParent);
        
        // CubeSet iters = new CubeSet(11, contained.getPoints());
        // foreach (MarchCube i in iters.getAllCubes()) {
        //     int caseVal = 0;
        //     for (int j = 0; j < 8; j++) {
        //         if (contained.contains(i.pointAt(j))) {
        //             caseVal += (int)Mathf.Round(Mathf.Pow(2,j));
        //         }
        //     }
        //     Vector3 spot = buildCubeMarchAt + roverSensor.transform.position + i.startPoint();
        //     Trig build = CubeCase.shape(caseVal);
        //     GameObject marched = Instantiate(marchable,spot,new Quaternion());
        //     marched.transform.parent = marchedParent.transform;
        //     Mesh tempMesh = new Mesh();
        //     tempMesh.vertices = build.points;
        //     tempMesh.triangles = build.order;
        //     tempMesh.RecalculateNormals();
        //     marched.GetComponent<MeshFilter>().mesh = tempMesh;
        // }
    }
}
