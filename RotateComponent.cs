using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

public class RotateComponent : MonoBehaviour
{
    public float rotationSpeed = 50f;
    
    private TransformAccessArray transformAccessArray;
    private RotateJob rotateJob;
    private JobHandle jobHandle;
    
    void Start()
    {
        transformAccessArray = new TransformAccessArray(1);
        transformAccessArray.Add(transform);
    }


    private ProfilerMarker profilerMarker = new ProfilerMarker("RotateComponent");
    private void Update()
    {
        profilerMarker.Begin();
        CalculateWithJob();
        profilerMarker.End();
    }
    
    private void CalculateWithJob()
    {
        rotateJob = new RotateJob
        {
            deltaTime = Time.deltaTime,
            rotationSpeed = rotationSpeed
        };

        jobHandle = rotateJob.Schedule(transformAccessArray);
        
        jobHandle.Complete();
    }

    void OnDestroy()
    {
        transformAccessArray.Dispose();
    }
}

[BurstCompile]
public struct RotateJob : IJobParallelForTransform
{
    [ReadOnly] public float deltaTime;
    [ReadOnly] public float rotationSpeed;

    public void Execute(int index, TransformAccess transform)
    {
        float zRotation = rotationSpeed * deltaTime;
        quaternion rotation = quaternion.Euler(0, 0, zRotation);
        transform.rotation = math.mul(transform.rotation, rotation);
    }
}