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

    void Awake () {
		CreatePart();
	}

    void CreatePart() {
        var go = new GameObject("Fractal Part"); //go short for gameObject
		go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
		go.AddComponent<MeshRenderer>().material = material;
    }
}