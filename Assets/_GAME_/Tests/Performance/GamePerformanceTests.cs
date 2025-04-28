using System;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine;
using UnityEngine.Profiling;

public class GamePerformanceTests
{
    [Test, Performance]
    public void MemoryAllocationTest()
    {
        Measure.Method(() =>
        {
            // Simulate object creation and destruction in Edit Mode
            for (int i = 0; i < 1000; i++)
            {
                var tempObject = new GameObject("TempObject");

                // Use DestroyImmediate in Edit Mode
                UnityEngine.Object.DestroyImmediate(tempObject);
            }
        })
        .WarmupCount(2)
        .MeasurementCount(10)
        .IterationsPerMeasurement(5)
        .Run();
    }

    [Test, Performance]
    public void ObjectCreationOverheadTest()
    {
        Measure.Method(() =>
        {
            // Simulate lightweight object creation
            for (int i = 0; i < 10000; i++)
            {
                var tempList = new System.Collections.Generic.List<int>();
                tempList.Add(i);
            }
        })
        .WarmupCount(3)
        .MeasurementCount(20)
        .IterationsPerMeasurement(10)
        .Run();
    }

    [Test, Performance]
    public void VectorCalculationPerformance()
    {
        Measure.Method(() =>
        {
            // Simulate vector calculations
            Vector2 position = Vector2.zero;
            Vector2 velocity = new Vector2(5f, 3f);

            for (int i = 0; i < 1000; i++)
            {
                position += velocity * Time.deltaTime;
                float distance = Vector2.Distance(position, Vector2.one);
                velocity = velocity.normalized * distance;
            }
        })
        .WarmupCount(5)
        .MeasurementCount(20)
        .IterationsPerMeasurement(10)
        .Run();
    }

    [Test, Performance]
    public void SimpleComputationTest()
    {
        Measure.Method(() =>
        {
            // Simulate basic computational task
            double result = 0;
            for (int i = 0; i < 10000; i++)
            {
                result += Math.Sin(i) * Math.Cos(i);
            }
        })
        .WarmupCount(3)
        .MeasurementCount(15)
        .IterationsPerMeasurement(5)
        .Run();
    }
}