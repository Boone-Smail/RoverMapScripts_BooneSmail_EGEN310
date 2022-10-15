using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempMarchEnvironment : MonoBehaviour
{

    public int Seed;
    public float timeConst;
    PointSet exists = new PointSet(41);
    public GameObject pointParent;
    public GameObject marchedParent;
    public GameObject marched;
    public GameObject existingPoint;
    public GameObject emptyPoint;
    public GameObject currentMarch;


    public void Start() {
        Random.InitState(Seed);
        for (float i = 0f; i < 5; i+=0.5f) {
            for (float j = 0f; j < 3; j+=0.5f) {
                for (float k = 0f; k < 5; k+=0.5f) {
                    Vector3 temp = new Vector3(i,j,k);
                    if (Random.Range(0,100) > 40) {
                        exists.addPoint(temp);
                        Instantiate(existingPoint,temp,new Quaternion()).transform.parent = pointParent.transform;
                    }
                    else {
                        Instantiate(emptyPoint,temp,new Quaternion()).transform.parent = pointParent.transform;
                    }
                }
            }
        }
        StartCoroutine(BuildSlowly());
    }

    IEnumerator BuildSlowly() {
        MarchCube[] marchThrough = new CubeSet(41,exists.getPoints()).getAllCubes();
        int index = 0;
        while (index < marchThrough.Length) {
            MarchCube temp = marchThrough[index];
            int caseVal = 0;
            for (int i = 0; i < 8; i++) {
                if (exists.contains(temp.pointAt(i))) {
                    caseVal += (int)Mathf.Pow(2,i);
                }
            }
            Trig thisMesh = CubeCase.shape(caseVal);
            Vector3 startPoint = temp.startPoint();
            currentMarch.transform.position = temp.startPoint();
            for (int i = 0; i < thisMesh.points.Length; i++) {
                thisMesh.points[i] += startPoint;
            }

            GameObject inst = Instantiate(marched);
            inst.transform.parent = marchedParent.transform;
            Mesh tempMesh = new Mesh();
            tempMesh.vertices = thisMesh.points;
            tempMesh.triangles = thisMesh.order;
            tempMesh.RecalculateNormals();
            inst.GetComponent<MeshFilter>().mesh = tempMesh;

            index++;
            yield return new WaitForSeconds(timeConst);
        }
    }

}
