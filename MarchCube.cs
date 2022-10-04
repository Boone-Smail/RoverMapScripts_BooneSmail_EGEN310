using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarchCube
{
    // Points move clockwise, top layer first,
    // starting at -x and -z
    List<Node> points;
    int a;
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

    public static MarchCube[] surround(Vector3 point) {
        MarchCube[] temp = new MarchCube[8];
        for (int i = 0; i < 8; i++) {
            temp[i] = new MarchCube(point + originShifts[i]);
        }
        return temp;
    }
}

public class CubeSet
{
    List<MarchCube>[] cubes;
    private int hashNum;
    
    private int hash(MarchCube cube) {
        return cube.GetHashCode()%hashNum;
    }
    
    public CubeSet(int _hash) {
        hashNum = _hash;
        cubes = new List<MarchCube>[_hash];
        for (int i = 0; i < hashNum; i++) {
            cubes[i] = new List<MarchCube>();
        }
    }

    public CubeSet(int _hash, Vector3[] insideMarch) {
        hashNum = _hash;
        cubes = new List<MarchCube>[hashNum];
        for (int i = 0; i < hashNum; i++) {
            cubes[i] = new List<MarchCube>();
        }
        foreach (Vector3 i in insideMarch) {
            MarchCube[] temp = MarchCube.surround(i);
            foreach (MarchCube j in temp) {
                add(j);
            }
        }
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
}