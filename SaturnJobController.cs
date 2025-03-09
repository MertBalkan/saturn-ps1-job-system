using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class SaturnJobController : MonoBehaviour
{
    public GameObject objectPrefab;
    public Transform[] objects;
    public Transform saturnParent;
    public int length;
    public float spawnRadius = 15f;
    public float innerRadius = 5f;

    public float rotationSpeed = 10f;

    private TransformAccessArray transformAccessArray;

    public Vector3 scale;

    private void Start()
    {
        objects = new Transform[length];

        for (int i = 0; i < length; i++)
        {
            Vector2 position;
            do
            {
                position = Random.insideUnitCircle * spawnRadius;
            } while (position.magnitude < innerRadius);

            GameObject obj = Instantiate(objectPrefab, new Vector3(position.x, 0, position.y), Quaternion.identity, saturnParent);
            obj.transform.localScale = Vector3.one * 5;
            objects[i] = obj.transform;
        }

        transformAccessArray = new TransformAccessArray(objects);

        saturnParent.transform.localPosition = Vector3.zero;
        saturnParent.transform.localRotation = Quaternion.Euler(new Vector3(20.9568558f, 162.013748f, 227.048294f));
        saturnParent.transform.localScale = scale;
    }

    private ProfilerMarker profilerMarker = new ProfilerMarker("SaturnJob");
    private void Update()
    {
        profilerMarker.Begin();
        CalculateWithJob();
        profilerMarker.End();
    }

    private void CalculateWithJob()
    {
        SaturnRotationJob job = new SaturnRotationJob
        {
            deltaTime = Time.deltaTime,
            rotationSpeed = rotationSpeed,
        };

        JobHandle handle = job.Schedule(transformAccessArray);
        handle.Complete();
    }

    private void OnDestroy()
    {
        if (transformAccessArray.isCreated)
        {
            transformAccessArray.Dispose();
        }
    }
}

[BurstCompile]
public struct SaturnRotationJob : IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float rotationSpeed;
    [ReadOnly] public uint seed;

    public void Execute(int index, TransformAccess transform)
    {
        uint uniqueSeed = math.max(seed + (uint)index, 1);
        var random = new Unity.Mathematics.Random(uniqueSeed);
        float3 randomValue = random.NextFloat3(-10f, 10f);
        
        float3 randomEuler = randomValue * rotationSpeed * deltaTime;
        quaternion rotationDelta = quaternion.Euler(math.radians(randomEuler));
        transform.rotation = math.mul(transform.rotation, rotationDelta);
    }
}
