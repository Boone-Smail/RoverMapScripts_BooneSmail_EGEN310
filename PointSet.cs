using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSet
{
    private int hashNum;
    private int idCounter;
    Dictionary<Node, int> idMap;
    List<Node>[] points;

    private int hash(Vector3 _point) {
        return (Node.asHash(_point) % hashNum);
    }

    public PointSet(int _hash) {
        hashNum = _hash;
        idCounter = 0;
        idMap = new Dictionary<Node, int>();
        points = new List<Node>[hashNum];
        for (int i = 0; i < hashNum; i++) {
            points[i] = new List<Node>();
        }
    }

    public bool contains(Vector3 _point) {
        return points[hash(_point)].Contains(Node.asTemp(_point));
    }

    public bool contains(Node _point) {
        return points[hash(_point.getPoint())].Contains(_point);
    }

    public bool addPoint(Vector3 _point) {
        if (!contains(_point)) {
            Node temp = new Node(_point, idCounter);
            idCounter++;
            points[hash(_point)].Add(temp);
            idMap.Add(temp, temp.getID());
            return true;
        }
        return false;
    }

    public Vector3[] getPoints() {
        Vector3[] temp = new Vector3[idMap.Keys.Count];
        foreach (Node i in idMap.Keys) {
            temp[idMap[i]] = i.getPoint();
        }
        return temp;
    }

    public int index(Vector3 point) {
        return idMap[Node.asTemp(point)];
    }

    public static PointSet operator +(PointSet A, PointSet B) {
        Vector3[] temp = B.getPoints();
        foreach (Vector3 i in temp) {
            A.addPoint(i);
        }
        return A;
    }

    public static PointSet operator +(PointSet A, Vector3[] B) {
        foreach (Vector3 i in B) {
            A.addPoint(i);
        }
        return A;
    }
}

// Vector3 with extra steps, it lets me hash
// floating point numbers in a more relative
// manner (within 0.1 of difference)
public class Node
{
    private Vector3 point;
    private int id;

    public Node(Vector3 _point, int _ID) {
        point = _point;
        id = _ID;
    }

    public int getID() {
        return id;
    }

    public Vector3 getPoint() {
        return point;
    }

    public static Node asTemp(Vector3 _point) {
        return new Node(_point, -1);
    }

    public override bool Equals(object obj)
    {
        Node temp = obj as Node;
        return (temp.getPoint() == point);
    }

    public override int GetHashCode()
    {
        int temp = 0;
        temp += Mathf.Abs(Mathf.RoundToInt(point.x*10));
        temp += Mathf.Abs(Mathf.RoundToInt(point.y*10));
        temp += Mathf.Abs(Mathf.RoundToInt(point.z*10));
        return temp;
    }

    public static int asHash(Vector3 _point) {
        int temp = 0;
        temp += Mathf.Abs(Mathf.RoundToInt(_point.x*10));
        temp += Mathf.Abs(Mathf.RoundToInt(_point.y*10));
        temp += Mathf.Abs(Mathf.RoundToInt(_point.z*10));
        return temp;
    }
}