using UnityEngine;

public class Fractal : MonoBehaviour {

    //how loopy do we want to go with the fractal
	[SerializeField, Range(1, 8)]
	int depth = 4;

    //materials and meshes for the fractal
    [SerializeField]
	Mesh mesh;

	[SerializeField]
	Material material;

    //storing some variables for ease of use later when we're creating the fractal
    static Vector3[] directions = {
		Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
	};

	static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 0f, -90f), Quaternion.Euler(0f, 0f, 90f),
		Quaternion.Euler(90f, 0f, 0f), Quaternion.Euler(-90f, 0f, 0f)
	};

    //for keeping track of all the fractal parts
    struct FractalPart {
		public Vector3 direction;
		public Quaternion rotation;
        public Transform transform;
	}
    FractalPart[][] parts; //2D array for each depth level parts 


    void Awake () {
        parts = new FractalPart[depth][];
        parts[0] = new FractalPart[1]; //because the root level only has one part (the major guy in the middle)

        //populating the rest of the array with the FractalPar structs
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new FractalPart[length];
        }

        //creating the parts themselves
        float scale = 1f;
		
        CreatePart(0, 0, scale);

        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) { //start at one cus we already created the first one
            scale *= 0.5f;
            FractalPart[] levelParts = parts[levelIndex];

            for (int fractalPartIndex = 0; fractalPartIndex < levelParts.Length; fractalPartIndex += 5) {
                for (int childIndex = 0; childIndex < 5; childIndex++) {
                    CreatePart(levelIndex, childIndex, scale);
                }
            }
        }
	}

    void CreatePart(int levelIndex, int childIndex, float scale) {
        var go = new GameObject("Fractal Part L" + levelIndex + " C" + childIndex); //go short for gameObject
		go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
		go.AddComponent<MeshRenderer>().material = material;
    }
}