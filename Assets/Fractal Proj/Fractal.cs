using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

using UnityEngine;

public class Fractal : MonoBehaviour {

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor {
        public float spinAngleDelta;
		public float scale;

        [ReadOnly]
		public NativeArray<FractalPart> parents;
		public NativeArray<FractalPart> parts;

        [WriteOnly]
		public NativeArray<float4x4> matrices;

        public void Execute (int i) {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];

            part.spinAngle += spinAngleDelta;
            part.worldRotation = mul(parent.worldRotation, 
                mul(part.rotation, quaternion.RotateY(part.spinAngle))); 
            part.worldPosition = parent.worldPosition +
                mul(parent.worldRotation, (1.5f * scale * part.direction));

            parts[i] = part;

            matrices[i] = float4x4.TRS(part.worldPosition, part.worldRotation, float3(scale));
        }
    }

    //how loopy do we want to go with the fractal
	[SerializeField, Range(1, 8)]
	int depth = 4;

    //materials and meshes for the fractal
    [SerializeField]
	Mesh mesh;

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
        public float spinAngle;
	}
    NativeArray<FractalPart>[] parts; //For each depth level parts

    NativeArray<float4x4>[] matrices;

    ComputeBuffer[] matricesBuffers;

    static readonly int matricesId = Shader.PropertyToID("_Matrices");
    static MaterialPropertyBlock propertyBlock;

    void OnEnable () {
        parts = new NativeArray<FractalPart>[depth];
        //parts[0] = new FractalPart[1]; //because the root level only has one part (the major guy in the middle)

        matrices = new NativeArray<float4x4>[depth];

        matricesBuffers = new ComputeBuffer[depth];
		int stride = 16 * 4;

        //populating the rest of the array with the FractalPar structs
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5) {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float4x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
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
    }

    void OnValidate () {
		if (parts != null && enabled) {
			OnDisable();
			OnEnable();
		}
	}

    void Update() {
        //the rotation speed
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;

        //rotating the root one first of all
        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle)));
        rootPart.worldPosition = transform.position;

        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        matrices[0][0] = float4x4.TRS(rootPart.worldPosition, 
            rootPart.worldRotation, float3(objectScale));
        float scale = objectScale;//scale for decreasing size

        JobHandle jobHandle = default;
        for (int levelIndex = 1; levelIndex < parts.Length; levelIndex++) {
            scale *= 0.5f;

            //Creating a new UpdateFractalLevelJob and setting all its values
            jobHandle = new UpdateFractalLevelJob {
				spinAngleDelta = spinAngleDelta,
				scale = scale,
				parents = parts[levelIndex - 1],
				parts = parts[levelIndex],
				matrices = matrices[levelIndex]
			}.Schedule(parts[levelIndex].Length, jobHandle);
        }
        jobHandle.Complete();

        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
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