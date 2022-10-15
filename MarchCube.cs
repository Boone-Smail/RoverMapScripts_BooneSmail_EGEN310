using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchCube
{
    public GameObject march;
    // Points move clockwise, top layer first,
    // starting at -x and -z
    List<Node> points;
    //Top left, clockwise on flat planes, going down
    static Vector3[] adds = new Vector3[] {
        new Vector3(0,0,0),
        new Vector3(0,0,0.5f),
        new Vector3(0.5f,0,0.5f),
        new Vector3(0.5f,0,0),
        new Vector3(0,-0.5f,0),
        new Vector3(0,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,0.5f),
        new Vector3(0.5f,-0.5f,0)
    };

    static Vector3[] originShifts = new Vector3[] {
        Vector3.zero,
        new Vector3(0,0,-0.5f),
        new Vector3(-0.5f,0,-0.5f),
        new Vector3(-0.5f,0,0),
        new Vector3(0,0.5f,0),
        new Vector3(0,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,-0.5f),
        new Vector3(-0.5f,0.5f,0)
    };

    public MarchCube(Vector3 frontTopRight) {
        points = new List<Node>();
        foreach (Vector3 i in adds) {
            points.Add(Node.asTemp(frontTopRight + i));
        }
    }

    public MarchCube(List<Node> _points) {
        if (_points.Count == 8) {
            points = _points;
        }
        else {
            points.Add(Node.asTemp(new Vector3(0,0.5f,0)));
            points.Add(Node.asTemp(new Vector3(0,0.5f,0.5f)));
            points.Add(Node.asTemp(new Vector3(0.5f,0.5f,0.5f)));
            points.Add(Node.asTemp(new Vector3(0.5f,0.5f,0f)));
            points.Add(Node.asTemp(new Vector3(0f,0f,0f)));
            points.Add(Node.asTemp(new Vector3(0f,0f,0.5f)));
            points.Add(Node.asTemp(new Vector3(0.5f,0f,0.5f)));
            points.Add(Node.asTemp(new Vector3(0.5f,0f,0f)));
        }
    }

    public List<Node> getPoints() {
        return points;
    }
    
    public Vector3[] getPointsAsVectors() {
        Vector3[] temp = new Vector3[8];
        for (int i = 0; i < 8; i++) {
            temp[i] = points[i].getPoint();
        }
        return temp;
    }

    public Vector3 startPoint() {
        return points[0].getPoint();
    }

    public Vector3 pointAt(int index) {
        return points[index].getPoint();
    }

    public override bool Equals(object obj)
    {
        MarchCube temp = obj as MarchCube;
        foreach (Node i in temp.getPoints()) {
            if (!points.Contains(i)) {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        int temp = 0;
        foreach (Node i in points) {
            temp += i.GetHashCode();
        }
        return temp;
    }

    public void setMarch(GameObject _march) {
        this.march = _march;
    }

    public static MarchCube[] surround(Vector3 point) {
        MarchCube[] temp = new MarchCube[8];
        for (int i = 0; i < 8; i++) {
            temp[i] = new MarchCube(point + originShifts[i]);
        }
        return temp;
    }

    public override string ToString() {
        string temp = "";
        temp += "\tHas GameObject: " + (this.march != null);
        temp+= "\t\t" + this.startPoint().ToString() + " <= (Start)";
        for (int i = 1; i < 8; i++) {
            temp+= "\n" + this.pointAt(i).ToString();
        }
        return temp;
    }
}

public class CubeSet
{
    List<MarchCube>[] cubes;
    private PointSet points;
    private int hashNum;
    
    private int hash(MarchCube cube) {
        return cube.GetHashCode()%hashNum;
    }

    public CubeSet(int _hash) {
        hashNum = _hash;
        cubes = new List<MarchCube>[_hash];
        for (int i = 0; i < _hash; i++) {
            cubes[i] = new List<MarchCube>();
        }
        points = new PointSet(31);
    }
    
    public CubeSet(int _hash, PointSet points) {
        hashNum = _hash;
        cubes = new List<MarchCube>[_hash];
        for (int i = 0; i < hashNum; i++) {
            cubes[i] = new List<MarchCube>();
        }
    }

    public CubeSet(int _hash, Vector3[] insideMarch) {
        hashNum = _hash;
        points = new PointSet(31);

        cubes = new List<MarchCube>[hashNum];
        for (int i = 0; i < hashNum; i++) {
            cubes[i] = new List<MarchCube>();
        }
        foreach (Vector3 i in insideMarch) {
            points.addPoint(i);
            MarchCube[] temp = MarchCube.surround(i);
            foreach (MarchCube j in temp) {
                add(j);
            }
        }
    }

    public MarchCube[] getAllCubes() {
        List<MarchCube> temp = new List<MarchCube>();
        foreach (List<MarchCube> i in cubes) {
            foreach (MarchCube j in i) {
                temp.Add(j);
            }
        }
        return temp.ToArray();
    }

    public bool contains(MarchCube cube) {
        return cubes[hash(cube)].Contains(cube);
    }

    public bool add(MarchCube cube) {
        if (!cubes[hash(cube)].Contains(cube)) {
            cubes[hash(cube)].Add(cube);
            return true;
        }
        return false;
    }

    public MarchCube get(MarchCube cube) {
        if (cubes[hash(cube)].Contains(cube)) {
            for (int i = 0; i < cubes[hash(cube)].Count; i++) {
                if (cubes[hash(cube)][i].Equals(cube)) {
                    return cubes[hash(cube)][i];
                }
            }
        }
        return null;
    }

    // Add all 8 cubes that surround a point, return true if
    // any are new to the instance
    public bool add(Vector3 surround) {
        // using number because there may be many true
        short count = 0;
        // Get the list of surrounding cubes
        MarchCube[] temp = MarchCube.surround(surround);
        foreach (MarchCube i in temp) {
            if (add(i)) {
                // Add one for every unique cube
                count++;
            }
        }
        // return true if ANY unique cubes were added
        if (count > 0) {
            return true;
        }
        else {
            return false;
        }
    }

    // An add function that already assumes the cube doesn't exist
    private void completeAdd(MarchCube added) {
        cubes[hash(added)].Add(added);
    }

    public void march(Vector3[] given, GameObject marchable, Vector3 startPos, GameObject parent) {
        
        // Set data
        MarchCube[] current = surrounding(given);
        points += given;

        // Change all existing, and add all new
        for (int i = 0; i < current.Length; i++) {
            
            // Set current march cube
            MarchCube temp = current[i];

            // Set GameObject data
            GameObject march = MonoBehaviour.Instantiate(marchable,temp.startPoint() + startPos,new Quaternion());
            Trig trig = CubeCase.shape(marchCase(temp));
            Mesh tempMesh = new Mesh();
            tempMesh.vertices = trig.points;
            tempMesh.triangles = trig.order;
            tempMesh.RecalculateNormals();
            march.GetComponent<MeshFilter>().mesh = tempMesh;

            march.transform.SetParent(parent.transform);


            if (contains(temp)) {
                temp = get(temp);
                // Debug.Log(temp.ToString());
                // temp.march.SetActive(false);
                string output = "\tOverwriting:\t" + (temp.march == null) + " | ";
                MonoBehaviour.Destroy(temp.march);
                output += (temp.march == null);
                output += "\n\t\tAt:\t\t" + temp.ToString();
                // Debug.Log(output);
                temp.setMarch(march);
            }
            else {
                temp.setMarch(march);
                add(temp);
            }
        }
        int total = 0;
        foreach (List<MarchCube> i in cubes) {
            total += i.Count;
        }
        Debug.Log("Total current marched cubes: " + total);
    }

    private MarchCube[] surrounding(Vector3[] given) {
        return new CubeSet(11,given).getAllCubes();
    }

    private MarchCube fromFirst(Vector3 start) {
        return new MarchCube(start);
    }

    private int marchCase(MarchCube cube) {
        int total = 0;
        for (int i = 0; i < 8; i++) {
            if (points.contains(cube.pointAt(i))) {
                total += Mathf.RoundToInt(Mathf.Pow(2,i));
            }
        }
        return total;
    }

    // Concatenate current point set and given
    private void addPoints(PointSet given) {
        points += given;
    }

    private void addPoints(Vector3[] given) {
        points += given;
    }

    public static CubeSet operator +(CubeSet A, CubeSet B) {
        foreach (MarchCube i in B.getAllCubes()) {
            A.add(i);
        }
        return A;
    }
}