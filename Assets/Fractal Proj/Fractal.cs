using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float3x3 = Unity.Mathematics.float3x3;
using float3x4 = Unity.Mathematics.float3x4;
using quaternion = Unity.Mathematics.quaternion;

using Random = UnityEngine.Random;

using UnityEngine;

public class Fractal : MonoBehaviour {

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor {
        public float deltaTime;
		public float scale;

        [ReadOnly]
		public NativeArray<FractalPart> parents;
		public NativeArray<FractalPart> parts;

        [WriteOnly]
		public NativeArray<float3x4> matrices;

        public void Execute (int i) {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];

            part.spinAngle += part.spinVelocity * deltaTime; //for rotating the fractal

            float3 upAxis = mul(mul(parent.worldRotation, part.rotation), up()); //for sagging parts
            float3 sagAxis = cross(up(), upAxis);

            float sagMagnitude = length(sagAxis);
            quaternion baseRotation;

            // if sagAxis > 0 then normalize it and sag it
            if (sagMagnitude > 0f) {
                sagAxis /= sagMagnitude; //normalizing if length > 0
                quaternion sagRotation = 
                    quaternion.AxisAngle(sagAxis, part.maxSagAngle * sagMagnitude); //rotation of 45deg
                baseRotation = mul(sagRotation, parent.worldRotation); //not relying on parent
            }
            else {
                baseRotation = parent.worldRotation;
            }

            //deciding on the rotation and position, as well as applying sagging here
            part.worldRotation = mul(baseRotation, 
                mul(part.rotation, quaternion.RotateY(part.spinAngle)));
            part.worldPosition = parent.worldPosition +
                mul(part.worldRotation, float3(0f, 1.5f * scale, 0f)); //only rotating w/ local up axis

            parts[i] = part;

            //comment here about the mtrices
            float3x3 r = float3x3(part.worldRotation) * scale;
            matrices[i] = float3x4(r.c0, r.c1, r.c2, part.worldPosition);
        }
    }

    //how loopy do we want to go with the fractal
	[SerializeField, Range(3, 8)]
	int depth = 6;

    //materials and meshes for the fractal
    [SerializeField]
	Mesh mesh, leafMesh;

	[SerializeField]
	Material material;

    //storing some variables for ease of use later when we're creating the fractal
    static float3[] directions = {
		up(), right(), left(), forward(), back()
	};

	static quaternion[] rotations = {
		quaternion.identity,
		quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
		quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
	};

    //for keeping track of all the fractal parts
    struct FractalPart {
		public float3 direction, worldPosition;
		public quaternion rotation, worldRotation;
        public float spinAngle, maxSagAngle, spinVelocity;
	}
    NativeArray<FractalPart>[] parts; //For each depth level parts

    NativeArray<float3x4>[] matrices;

    ComputeBuffer[] matricesBuffers;

    [SerializeField]
    Gradient gradientA, gradientB;

    [SerializeField]
    Color leafColorA, leafColorB;

    [SerializeField, Range(0f, 90f)]
    float maxSagAngleA = 15f, maxSagAngleB = 25f;

    [SerializeField, Range(0f, 90f)]
	float spinSpeedA = 20f, spinSpeedB = 25f;

    [SerializeField, Range(0f, 1f)]
	float reverseSpinChance = 0.25f;

    static readonly int
        colorAId = Shader.PropertyToID("_ColorA"),
        colorBId = Shader.PropertyToID("_ColorB"),
        matricesId = Shader.PropertyToID("_Matrices"),
        sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");

    static MaterialPropertyBlock propertyBlock;

    Vector4[] sequenceNumbers;

    void OnEnable () {
        parts = new NativeArray<FractalPart>[depth];
        //parts[0] = new FractalPart[1]; //because the root level only has one part (the major guy in the middle)

        matrices = new NativeArray<float3x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];

        sequenceNumbers = new Vector4[depth];

		int stride = 12 * 4;

        //populating the rest of the array with the FractalPar structs
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }

        //creating the parts themselves
       parts[0][0] = CreatePart(0); //the first big part in the middle

        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) { //start at one cus we already created the first one
            NativeArray<FractalPart> levelParts = parts[levelIndex];

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
            parts[i].Dispose();
            matrices[i].Dispose();
		}
        parts = null;
		matrices = null;
		matricesBuffers = null;
        sequenceNumbers = null;
    }

    void OnValidate () {
		if (parts != null && enabled) {
			OnDisable();
			OnEnable();
		}
	}

    void Update() {
        //the rotation speed
        float deltaTime = Time.deltaTime;

        //rotating the root one first of all
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += rootPart.spinVelocity * deltaTime;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;

        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        float3x3 r = float3x3(rootPart.worldRotation) * objectScale;
        matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.worldPosition);

        float scale = objectScale;//scale for decreasing size

        JobHandle jobHandle = default;
        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) {
            scale *= 0.5f;

            //Creating a new UpdateFractalLevelJob and setting all its values
            jobHandle = new UpdateFractalLevelJob {
				deltaTime = deltaTime,
				scale = scale,
				parents = parts[levelIndex - 1],
				parts = parts[levelIndex],
				matrices = matrices[levelIndex]
			}.ScheduleParallel(parts[levelIndex].Length, 5, jobHandle);
        }
        jobHandle.Complete();

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        int leafIndex = matricesBuffers.Length - 1;
        for (int i = 0; i < matricesBuffers.Length; i++) {
			ComputeBuffer buffer = matricesBuffers[i];

			buffer.SetData(matrices[i]);

            Color colorA, colorB;
            Mesh instanceMesh;
            if (i == leafIndex) { //if it's a leaf
                colorA = leafColorA;
                colorB = leafColorB;
                instanceMesh = leafMesh;
            }
            else { //if it's a branch inside
                float gradientInterpolator = i / (matricesBuffers.Length - 2f);
                colorA = gradientA.Evaluate(gradientInterpolator);
                colorB = gradientB.Evaluate(gradientInterpolator);
                instanceMesh = mesh;
            }
            propertyBlock.SetColor(colorAId, colorA);
            propertyBlock.SetColor(colorBId, colorB);

			propertyBlock.SetBuffer(matricesId, buffer);
            propertyBlock.SetVector(sequenceNumbersId, sequenceNumbers[i]);

			Graphics.DrawMeshInstancedProcedural(
                instanceMesh, 0, material, bounds, buffer.count, propertyBlock);
		}
    }

    FractalPart CreatePart(int childIndex) => new FractalPart{
        direction = directions[childIndex],
        rotation = rotations[childIndex],
        maxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
        spinVelocity = (Random.value < reverseSpinChance ? -1f : 1f) * 
            radians(Random.Range(spinSpeedA, spinSpeedB))
    };
}