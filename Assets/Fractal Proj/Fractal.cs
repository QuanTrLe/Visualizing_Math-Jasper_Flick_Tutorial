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
		public Vector3 direction, worldPosition;
		public Quaternion rotation, worldRotation;
        public float spinAngle;
	}
    FractalPart[][] parts; //2D array for each depth level parts 

    Matrix4x4[][] matrices;

    ComputeBuffer[] matricesBuffers;

    static readonly int matricesId = Shader.PropertyToID("_Matrices");
    static MaterialPropertyBlock propertyBlock;

    void OnEnable () {
        parts = new FractalPart[depth][];
        parts[0] = new FractalPart[1]; //because the root level only has one part (the major guy in the middle)

        matrices = new Matrix4x4[depth][];

        matricesBuffers = new ComputeBuffer[depth];
		int stride = 16 * 4;

        //populating the rest of the array with the FractalPar structs
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new FractalPart[length];
            matrices[i] = new Matrix4x4[length];
            matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        //creating the parts themselves
       parts[0][0] = CreatePart(0); //the first big part in the middle

        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) { //start at one cus we already created the first one
            FractalPart[] levelParts = parts[levelIndex];

            for (int fractalPartIndex = 0; fractalPartIndex < levelParts.Length; fractalPartIndex += 5) {
                for (int childIndex = 0; childIndex < 5; childIndex++) {
                    levelParts[fractalPartIndex + childIndex] = CreatePart(childIndex);
                }
            }
        }

        //initializing a material property block to link each buffer to a specific draw commands
        propertyBlock ??= new MaterialPropertyBlock();
	}

    void OnDisable() {
        for (int i = 0; i < matricesBuffers.Length; i++) {
			matricesBuffers[i].Release();
		}
        parts = null;
		matrices = null;
		matricesBuffers = null;
    }

    void OnValidate () {
		if (parts != null && enabled) {
			OnDisable();
			OnEnable();
		}
	}

    void Update() {
        //the rotation speed
        float spinAngleDelta = 22.5f * Time.deltaTime;

        //rotating the root one first of all
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = rootPart.rotation * Quaternion.Euler(0f, rootPart.spinAngle, 0f);
        parts[0][0] = rootPart;
        matrices[0][0] = Matrix4x4.TRS(rootPart.worldPosition, rootPart.worldRotation, Vector3.one);

        float scale = 1f;//scale for decreasing size

        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) {
            scale *= 0.5f;

            FractalPart[] parentParts = parts[levelIndex - 1];
            FractalPart[] levelParts = parts[levelIndex];
            Matrix4x4[] levelMatrices = matrices[levelIndex];

            for (int fractalPartIndex = 0; fractalPartIndex < levelParts.Length; fractalPartIndex++) {
                FractalPart parent = parentParts[fractalPartIndex / 5];
                FractalPart part = levelParts[fractalPartIndex];

                part.spinAngle += spinAngleDelta;
                part.worldRotation = parent.worldRotation * (part.rotation * Quaternion.Euler(0f, part.spinAngle, 0f)); 
                part.worldPosition = parent.worldPosition +
                    parent.worldRotation * (1.5f * scale * part.direction);

                levelParts[fractalPartIndex] = part;

                levelMatrices[fractalPartIndex] = Matrix4x4.TRS(part.worldPosition, part.worldRotation, scale * Vector3.one);
            }
        }

        var bounds = new Bounds(Vector3.zero, 3f * Vector3.one);
        for (int i = 0; i < matricesBuffers.Length; i++) {
			ComputeBuffer buffer = matricesBuffers[i];
			buffer.SetData(matrices[i]);
			propertyBlock.SetBuffer(matricesId, buffer);
			Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, buffer.count, propertyBlock);
		}
    }

    FractalPart CreatePart(int childIndex) => new FractalPart{
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };
}