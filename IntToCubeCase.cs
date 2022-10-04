using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntToCubeCase : MonoBehaviour
{
    public GameObject[] Points;
    public GameObject tangible;
    public Material activeMat;
    public Material inactiveMat;
    private Mesh tempMesh;
    [Range(0,255)]
    public int cubeCase;
    public int last;
    public bool iterateThrough;
    private bool[] boolPoint;
    private int[] boolVals = new int[] {
        1,
        2,
        4,
        8,
        16,
        32,
        64,
        128
    };

    public void Start() {
        last = 0;
        boolPoint = new bool[8];
        StartCoroutine(step());
        tempMesh = new Mesh();
    }

    public void Update() {
        if (cubeCase != last) {
            last = cubeCase;
            resetBools();
            updateVerts();
            drawShape();
        }
    }

    public void drawShape() {
        Trig data = CubeCase.shape(cubeCase);
        tempMesh.Clear();
        tempMesh.vertices = data.points;
        tempMesh.triangles = data.order;
        tangible.GetComponent<MeshFilter>().mesh = tempMesh;
        tangible.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        
        // string output = "";
        // foreach (Vector3 i in tempMesh.vertices) {
        //     output += "(" + i.x + " " + i.y + " " + i.z + ") ";
        // }
        // output += "\n";
        // foreach (int i in tempMesh.triangles) {
        //     output += i + " ";
        // }
        // Debug.Log(output);
    }

    public void updateVerts() {
        int count = cubeCase;
        for (int i = 7; i >= 0; i--) {
            //Debug.Log(count + " >= " + boolVals[i] + " = " + (count>=boolVals[i]));
            if (count >= boolVals[i]) {
                count -= boolVals[i];
                boolPoint[i] = true;
                Points[i].GetComponent<MeshRenderer>().material = activeMat;
                //Debug.Log("Point value " + boolVals[i] + " changed");
            }
        }
        //Debug.Log("Here");
    }

    public void resetBools() {
        for (int i = 0; i < 8; i++) {
            boolPoint[i] = false;
            Points[i].GetComponent<MeshRenderer>().material = inactiveMat;
        }
    }

    public IEnumerator step() {
        yield return new WaitForSeconds(1f);
        while (true) {
            if(iterateThrough) {
                cubeCase++;
                if (cubeCase > 255) {
                    cubeCase = 0;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }
}

public static class CubeCase {
    // Clockwise, starting left (front left for vertical edges) and going down in layers
    // "em" = "edge midpoints"
    static Vector3[] em = new Vector3[] {
        new Vector3(0, 0, 0.25f),       //0
        new Vector3(0.25f, 0, 0.5f),    //1
        new Vector3(0.5f, 0, 0.25f),    //2
        new Vector3(0.25f, 0, 0),       //3
        new Vector3(0, -0.25f, 0),      //4
        new Vector3(0, -0.25f, 0.5f),   //5
        new Vector3(0.5f, -0.25f, 0.5f),//6
        new Vector3(0.5f, -0.25f, 0),   //7
        new Vector3(0, -0.5f, 0.25f),   //8
        new Vector3(0.25f, -0.5f, 0.5f),//9
        new Vector3(0.5f, -0.5f, 0.25f),//10
        new Vector3(0.25f, -0.5f, 0)    //11
    };

    //To build more complex cases
    static List<Vector3> pointTemp = new List<Vector3>();
    static List<int> tempInd = new List<int>();
    // Default triangle case, connects first to second to third
    static int[] def = new int[] {0,1,2};
    //values of an em vertex if the cube rolls to the left (counter-clockwise)
    private static int[] emRoll = new int[] {8,5,0,4,11,9,1,3,10,6,2,7};
    //values of an em vertex if the cube summersaults forward
    private static int[] emSault = new int[] {5,9,6,1,0,8,10,2,4,11,7,3};
    //values of an em vertex mirrorred front/back
    private static int[] emMirror = new int[] {0,3,2,1,5,4,7,6,8,11,10,9};
    //values of an em vertex mirrored right/left
    private static int[] emReflect = new int[] {2,1,0,3,7,6,5,4,10,9,8,11};
    //values of an em vertex mirrored up/down
    //Yes this is a kingdom hearts reference, no I don't feel bad about it
    private static int[] emNobody = new int[] {8,9,10,11,4,5,6,7,0,1,2,3};
    private static List<Vector3> temp = new List<Vector3>();
    private static Trig tempTrig;

    // First occurences of specific shapes are marked with comments.
    // Most other shapes are transformations of original ones.
    public static Trig shape(int cubeCase) {
        switch (cubeCase) {
            case (0 | 255):
                return new Trig();
            case 1:     // Single corner
                return new Trig(Assess(new int[] {3,0,4}), def);
            case 2:
                return new Trig(rotateClockwise(new int[] {3,0,4}), def);
            case 3:     // Triangle shaft
                return new Trig(new Vector3[] {em[4], em[3], em[1]}, def) +
                    new Trig(new Vector3[] {em[4], em[1], em[5]}, def);
            case 4:
                return new Trig(rotateClockwise(new int[] {3,0,4},2), def);
            case 5:     //Same plane, opposite corners
                return shape(1) + shape(4);
            case 6:
                return new Trig(rotateClockwise(new int[] {4,3,1}), def) +
                    new Trig(rotateClockwise(new int[] {4,1,5}), def);
            case 7:     // Tri corner (L)
                return new Trig(Assess(new int[] {4,6,5}),def) +
                    new Trig(Assess(new int[] {3,2,4}), def) +
                    new Trig(Assess(new int[] {2,6,4}), def);
            case 8:
                return new Trig(rotateClockwise(new int[] {3,0,4},3), def);
            case 9:
                return new Trig(rotateClockwise(new int[] {4,3,1},3), def) +
                    new Trig(rotateClockwise(new int[] {4,1,5},3), def);
            case 10:// First test of interpret
                tempTrig = shape(5);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 11:
                tempTrig = shape(7);
                tempTrig.points = rotateClockwise(interpret(), 3);
                return tempTrig;
            case 12:
                tempTrig = shape(3);
                tempTrig.points = rotateClockwise(interpret(), 2);
                return tempTrig;
            case 13:
                tempTrig = shape(7);
                tempTrig.points = rotateClockwise(interpret(), 2);
                return tempTrig;
            case 14:
                tempTrig = shape(7);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 15:        // Flat surface
                return new Trig(Assess(new int[] {4,6,5}), def) +
                    new Trig(Assess(new int[] {4,7,6}), def);
            case 16:
                tempTrig = shape(1);
                tempTrig.points = sault(interpret(),3);
                return tempTrig;
            case 17:
                tempTrig = shape(3);
                tempTrig.points = sault(interpret(), 3);
                return tempTrig;
            case 18:
                tempTrig = shape(5);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 19:
                tempTrig = shape(13);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 20:        // True opposite corners
                return shape(16) + shape(4);
            case 21:        // Pillar with opposite UPPER corner
                return shape(17) + shape(4);
            case 22:
                return shape(16) + shape(6);
            case 23:        // WaLLuigi (This one sucks, use as much transformation as possible)
                return new Trig(Assess(new int[] {11,3,8}), def) +
                    new Trig(Assess(new int[] {3,5,8}), def) + 
                    new Trig(Assess(new int[] {5,2,6}), def) +
                    new Trig(Assess(new int[] {5,3,2}), def);
            case 24:
                tempTrig = shape(5);
                tempTrig.points = sault(interpret(), 3);
                return tempTrig;
            case 25: 
                tempTrig = shape(7);
                tempTrig.points = sault(interpret(), 3);
                return tempTrig;
            case 26:        // Three individual corners
                return shape(16) + shape(8) + shape(2);
            case 27:        // Hex (BEEG BEEG CORNER)
                return new Trig(Assess(new int[] {2,11,7}), def) +
                    new Trig(Assess(new int[] {2,1,11}), def) +
                    new Trig(Assess(new int[] {1,8,11}), def) +
                    new Trig(Assess(new int[] {1,5,8}), def);
            case 28:        // Pillar with opposite LOWER corner
                return shape(12) + shape(16);
            case 29:        // Inverse WaLLuigi
                tempTrig = shape(23);
                tempTrig.points = fall(interpret());
                tempTrig.points = rotateClockwise(interpret(),3);
                tempTrig.points = roll(interpret(),2);
                invertNormals();
                return tempTrig;
            case 30:
                return shape(14) + shape(16);
            case 31:    //First instance of inversion without reversing
                tempTrig = shape(14);
                tempTrig.points = fall(interpret());
                return tempTrig;
            case 32:
                tempTrig = shape(2);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 33:
                return shape(1) + shape(32);
            case 34:
                tempTrig = shape(3);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 35:
                tempTrig = shape(14);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 36:
                return shape(4) + shape(32);
            case 37:
                return shape(1) + shape(36);
            case 38:
                tempTrig = shape(11);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 39:
                tempTrig = shape(27);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 40:
                tempTrig = shape(20);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 41:
                tempTrig = shape(21);
                tempTrig.points = rotateClockwise(interpret(), 3);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 42:
                tempTrig = shape(21);
                tempTrig.points = mirror(interpret());
                invertNormals();
                return tempTrig;
            case 43:
                tempTrig = shape(23);
                tempTrig.points = mirror(interpret());
                invertNormals();
                return tempTrig;
            case 44:
                tempTrig = shape(21);
                tempTrig.points = rotateClockwise(interpret(), 3);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 45:
                return shape(32) + shape(13);
            case 46:
                tempTrig = shape(29);
                tempTrig.points = mirror(interpret());
                invertNormals();
                return tempTrig;
            case 47:
                tempTrig = shape(31);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 48:
                tempTrig = shape(3);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 49:
                tempTrig = shape(11);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 50:
                tempTrig = shape(7);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 51:
                tempTrig = shape(15);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 52:
                tempTrig = shape(42);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 53:
                return shape(49) + shape(4);
            case 54:
                tempTrig = shape(43);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 55:
                tempTrig = shape(13);
                tempTrig.points = roll(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 56:
                return shape(48) + shape(8);
            case 57:
                tempTrig = shape(23);
                tempTrig.points = sault(interpret(), 3);
                return tempTrig;
            case 58:
                return shape(50) + shape(8);
            case 59:
                tempTrig = shape(14);
                tempTrig.points = roll(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 60:
                return shape(48) + shape(12);
            case 61:
                tempTrig = shape(32) + shape(12);
                tempTrig.points = roll(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 62:
                tempTrig = shape(28);
                tempTrig.points = roll(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 63:
                tempTrig = shape(48);
                tempTrig.points = roll(interpret(), 1);
                invertNormals();
                return tempTrig;
            case 64:
                tempTrig = shape(32);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 65:
                tempTrig = shape(40);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 66:
                tempTrig = shape(5);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 67:
                tempTrig = shape(21);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 68:
                tempTrig = shape(34);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 69:
                tempTrig = shape(28);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 70:
                tempTrig = shape(13);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 71:
                tempTrig = shape(29);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 72:
                tempTrig = shape(18);
                tempTrig.points = roll(interpret(), 2);
                return tempTrig;
            case 73:
                tempTrig = shape(21);
                tempTrig.points = roll(interpret(), 3);
                return tempTrig;
            case 74:
                tempTrig = shape(37);
                tempTrig.points = roll(interpret(), 3);
                return tempTrig;
            case 75:
                return shape(11) + shape(64);
            case 76:
                tempTrig = shape(50);
                tempTrig.points = roll(interpret(), 2);
                return tempTrig;
            case 77:
                tempTrig = shape(23);
                tempTrig.points = roll(interpret(), 3);
                return tempTrig;
            case 78:
                tempTrig = shape(39);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 79:    // First instance of mirror cheating
                tempTrig = shape(11);
                tempTrig.points = fall(interpret());
                return tempTrig;
            case 80:
                tempTrig = shape(33);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 81:
                tempTrig = shape(52);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 82:
                return shape(64) + shape(16) + shape(2);
            case 83:
                tempTrig = shape(45);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 84:
                tempTrig = shape(44);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 85:
                tempTrig = shape(60);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 86:
                tempTrig = shape(45);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 87:
                tempTrig = shape(81);
                tempTrig.points = rotateClockwise(interpret(),3);
                invertNormals();
                return tempTrig;
            case 88:
                return shape(64) + shape(16) + shape(8);
            case 89:
                return shape(25) + shape(64);
            case 90:        // Full quad oposing corners
                return shape(2) + shape(8) + shape(16) + shape(64);
            case 91:        // Hex and opposite corner
                return shape(27) + shape(64);
            case 92: // First instance of += on Trig; it works.
                tempTrig = shape(7);
                tempTrig.points = roll(interpret(),3);
                tempTrig += shape(16);
                return tempTrig;
            case 93:
                tempTrig = shape(81);
                tempTrig.points = rotateClockwise(interpret());
                invertNormals();
                return tempTrig;
            case 94:
                return shape(78) + shape(16);
            case 95:
                tempTrig = shape(80);
                tempTrig.points = rotateClockwise(interpret());
                invertNormals();
                return tempTrig;
            case 96:
                tempTrig = shape(6);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 97:
                tempTrig = shape(42);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 98:
                tempTrig = shape(7);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 99:
                tempTrig = shape(23);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 100:
                tempTrig = shape(14);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 101:
                return shape(100) + shape(1);
            case 102:
                tempTrig = shape(15);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 103:
                tempTrig = shape(13);
                tempTrig.points = sault(interpret(),3);
                invertNormals();
                return tempTrig;
            case 104:
                return shape(96) + shape(8);
            case 105:
                return shape(96) + shape(9);
            case 106:
                return shape(98) + shape(8);
            case 107:
                tempTrig = shape(73);
                tempTrig.points = sault(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 108:
                tempTrig = shape(54);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 109:
                tempTrig = shape(41);
                tempTrig.points = sault(interpret(), 3);
                invertNormals();
                return tempTrig;
            case 110:
                tempTrig = shape(25);
                tempTrig.points = roll(interpret(),1);
                invertNormals();
                return tempTrig;
            case 111: // Mirror cheat
                tempTrig = shape(9);
                tempTrig.points = fall(interpret());
                return tempTrig;   
            case 112:  
                tempTrig = shape(7);
                tempTrig.points = fall(interpret());
                invertNormals();
                return tempTrig;
            case 113:
                tempTrig = shape(43);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 114:
                tempTrig = shape(39);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 115: // Mirror cheat
                tempTrig = shape(19);
                tempTrig.points = reflect(interpret());
                return tempTrig;      
            case 116:
                tempTrig = shape(99);
                tempTrig.points = roll(interpret(), 1);
                return tempTrig;
            case 117: // Mirror cheat
                tempTrig = shape(69);
                tempTrig.points = mirror(interpret());
                return tempTrig;
            case 118: // Mirror cheat
                tempTrig = shape(70);
                tempTrig.points = mirror(interpret());
                return tempTrig;
            case 119: // Mirror cheat
                tempTrig = shape(68);
                tempTrig.points = mirror(interpret());
                return tempTrig;
            case 120:
                return shape(8) + shape(112);
            case 121: // Mirror cheat
                tempTrig = shape(104);
                tempTrig.points = fall(interpret());
                return tempTrig;
            case 122:
                return shape(114) + shape(8);
            case 123: // Mirror cheat
                tempTrig = shape(18);
                tempTrig.points = reflect(interpret());
                return tempTrig;
            case 124: // Mirror cheat
                tempTrig = shape(28);
                tempTrig.points = reflect(interpret());
                return tempTrig;
            case 125: // Mirror cheat
                tempTrig = shape(65);
                tempTrig.points = mirror(interpret());
                return tempTrig;
            case 126:
                tempTrig = shape(66);
                tempTrig.points = mirror(interpret());
                return tempTrig;
            case 127: // Mirror cheat
                tempTrig = shape(8);
                tempTrig.points = fall(interpret());
                return tempTrig;
            case 128:
                tempTrig = shape(16);
                tempTrig.points = roll(interpret(),1);
                return tempTrig;
            case 129:
                tempTrig = shape(80);
                tempTrig.points = sault(interpret(), 1);
                return tempTrig;
            case 130:
                tempTrig = shape(65);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 131:
                return shape(3) + shape(128);
            case 132:
                return shape(4) + shape(128);
            case 133:
                return shape(1) + shape(4) + shape(128);
            case 134:
                return shape(6) + shape(128);
            case 135:
                return shape(7) + shape(128);
            case 136:
                tempTrig = shape(68);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 137:
                tempTrig = shape(76);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 138:
                tempTrig = shape(69);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 139:
                tempTrig = shape(57);
                tempTrig.points = roll(interpret(), 3);
                return tempTrig;
            case 140:
                tempTrig = shape(11);
                tempTrig.points = roll(interpret(),3);
                return tempTrig;
            case 141:
                tempTrig = shape(78);
                tempTrig.points = rotateClockwise(interpret());
                return tempTrig;
            case 142:
                tempTrig = shape(43);
                tempTrig.points = roll(interpret(), 3);
                return tempTrig;
            case 143:
                tempTrig = shape(7);
                tempTrig.points = fall(interpret());
                return tempTrig;
            case 144:
                tempTrig = shape(96);
                tempTrig.points = sault(interpret(),1);
                return tempTrig;
            case 145:
                tempTrig = shape(11);
                tempTrig.points = sault(interpret(), 3);
                return tempTrig;
        }
        return new Trig();
    }

    // Rolling and rotating an object doesn't change how its vertices are connected
    // This means we can play with the vertices positions keeping them
    // relative of each other without consequence, as the order
    // never needs to be changed.
    private static int[] interpret(Trig given) {
        int[] build = new int[given.points.Length];
        for (int i = 0; i < build.Length; i++) {
            for (int j = 0; j < 12; j++) {
                if (given.points[i] == em[j]) {
                    build[i] = j;
                    // Debug.Log(given.points[i].ToString() + " " + em[j].ToString());
                    break;
                }
            }
        }
        return build;
    }

    // interpret always interprets tempTrig anyways, default method
    private static int[] interpret() {
        int[] build = new int[tempTrig.points.Length];
        for (int i = 0; i < build.Length; i++) {
            for (int j = 0; j < 12; j++) {
                if (tempTrig.points[i] == em[j]) {
                    build[i] = j;
                    // Debug.Log(tempTrig.points[i].ToString() + " " + em[j].ToString());
                    break;
                }
            }
        }
        return build;
    }

    private static Vector3[] Assess(int[] ind) {
        temp.Clear();
        for (int i = 0; i < ind.Length; i++) {
            temp.Add(em[ind[i]]);
        }
        return temp.ToArray();
    }

    private static Vector3[] rotateClockwise(int[] og) {
        for (int i = 0; i < og.Length; i++) {
            og[i] = og[i] + 1;
            if (og[i]%4 == 0 && og[i] != 0) {
                og[i] = og[i] - 4;
            }
        }
        return Assess(og);
    }

    // There's a way to do these with bitwise manipulation, there HAS to be
    private static Vector3[] rotateClockwise(int[] og, int iterations) {
        iterations %= 4;
        for (int i = 0; i < iterations; i++) {
            for (int j = 0; j < og.Length; j++) {
                og[j] = og[j] + 1;
                if (og[j]%4 == 0 && og[j] != 0) {
                    og[j] = og[j] - 4;
                }
            }
        }
        return Assess(og);
    }
    
    private static Vector3[] roll(int[] og, int iterations) {
        iterations %= 4;
        for (int i = 0; i < iterations; i++) {
            for (int j = 0; j < og.Length; j++) {
                og[j] = emRoll[og[j]];
            }
        }
        return Assess(og);
    }

    public static Vector3[] sault(int[] og, int iterations) {
        iterations %= 4;
        for (int i = 0; i < iterations; i++) {
            for (int j = 0; j < og.Length; j++) {
                og[j] = emSault[og[j]];
            }
        }
        return Assess(og);
    }

    // ANY REFLECTION CALLS FOR AN INVERSION OF NORMALS
    // DOUBLE REFLECTING IS A DOUBLE NEGATIVE FOR INVERSION
    // DO NOT ADD SHAPES WHEN INVERTING UNLESS BOTH SHAPES ARE INVERTED

    // front/back
    private static Vector3[] mirror(int[] og) {
        for (int i = 0; i < og.Length; i++) {
            og[i] = emMirror[og[i]];
        }
        return Assess(og);
    }

    // right/left
    private static Vector3[] reflect(int[] og) {
        for (int i = 0; i < og.Length; i++) {
            og[i] = emReflect[og[i]];
        }
        return Assess(og);
    }

    // top/bottom
    private static Vector3[] fall(int[] og) {
        for (int i = 0; i < og.Length; i++) {
            og[i] = emNobody[og[i]];
        }
        return Assess(og);
    }

    private static void invertNormals() {
        int count = 0;
        int cover;
        while (count < tempTrig.order.Length) {
            cover = tempTrig.order[count+1];
            tempTrig.order[count+1] = tempTrig.order[count+2];
            tempTrig.order[count+2] = cover;
            count+=3;
        }
    }

    private static int[] invertNormals(int[] og) {
        int count = 0;
        int cover;
        while (count < og.Length) {
            cover = og[count+1];
            og[count+1] = og[count+2];
            og[count+2] = cover;
            count+=3;
        }
        return og;
    }
}

public class Trig {
    public Vector3[] points;
    public int[] order;
    //Not used outside of operators / more internal functions
    private static Dictionary<int, int> maps = new Dictionary<int, int>();
    private static List<int> trigBuild = new List<int>();

    public Trig() {
        points = new Vector3[0];
        order = new int[0];
    }

    public Trig(Vector3[] _points, int[] _order) {
        points = _points;
        order = _order;
    }

    public static Trig operator +(Trig a, Trig b) => (combine(a,b));

    static Trig combine(Trig a, Trig b) {
        // Empty maps and trigBuilds
        maps.Clear();
        trigBuild.Clear();
        // Make only one instance of duplicate points using PointSet
        PointSet temp = new PointSet(1);
        foreach(Vector3 i in a.points) {
            temp.addPoint(i);
        }
        foreach(Vector3 i in b.points) {
            temp.addPoint(i);
        }
        // Start with triangle indeces of the original
        foreach(int i in a.order) {
            trigBuild.Add(i);
        }
        // Map indeces of second trig to indeces of new monster
        foreach(int i in b.order) {
            if (!maps.ContainsKey(i)) {
                maps.Add(i, temp.index(b.points[i]));
                //Debug.Log(i.ToString() + " " + b.points[i].ToString() + " " + temp.index(b.points[i]).ToString());
            }
        }
        // Create that amalgamation, you animal
        foreach(int i in b.order) {
            trigBuild.Add(maps[i]);
        }
        // Dear god look at what you've done
        return new Trig(temp.getPoints(), trigBuild.ToArray());
    }

    public override string ToString()
    {
        string output = "";
        for (int i = 0; i < points.Length; i++) {
            output += points[i].ToString() + " ";
        }
        output += "\n";
        foreach (int i in order) {
            output += i + " ";
        }
        return output;
    }
}

// public static class EdgeCubeBuild {
//     // Clockwise, starting left (front left for vertical edges) and going down in layers
//     // "em" = "edge midpoints"
//     static Vector3[] em = new Vector3[] {
//         new Vector3(0, 0, 0.25f),       //0
//         new Vector3(0.25f, 0, 0.5f),    //1
//         new Vector3(0.5f, 0, 0.25f),    //2
//         new Vector3(0.25f, 0, 0),       //3
//         new Vector3(0, -0.25f, 0),      //4
//         new Vector3(0, -0.25f, 0.5f),   //5
//         new Vector3(0.5f, -0.25f, 0.5f),//6
//         new Vector3(0.5f, -0.25f, 0),   //7
//         new Vector3(0, -0.5f, 0.25f),   //8
//         new Vector3(0.25f, -0.5f, 0.5f),//9
//         new Vector3(0.5f, -0.5f, 0.25f),//10
//         new Vector3(0.25f, -0.5f, 0)    //11
//     };

// }
