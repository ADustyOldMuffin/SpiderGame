using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FastIK : MonoBehaviour
{
    /// <summary>
    /// Chain length of bones
    /// </summary>
    public int chainLength = 4;
    
    /// <summary>
    /// Solver iterations per update
    /// </summary>
    public int iterations = 10;
    
    /// <summary>
    /// Target the chain should be bent to
    /// </summary>
    public Transform target;
    public Transform pole;
    
    /// <summary>
    /// Distance when the solver stops
    /// </summary>
    public float delta = 0.001f;
    
    /// <summary>
    ///Strength of going back to the start position
    /// </summary>
    [Range(0, 1)]
    public float snapBackStrength = 1f;

    private float[] bonesLength;
    private float completeLength;
    private Transform[] bones;
    private Vector3[] positions;
    private Vector3[] startDirectionSucc;
    private Quaternion[] startRotationBone;
    private Quaternion startRotationTarget;
    private Quaternion startRotationRoot;

    private void Awake()
    {
        Init();
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void Init()
    {
        bones = new Transform[chainLength + 1];
        positions = new Vector3[chainLength + 1];
        bonesLength = new float[chainLength];
        startDirectionSucc = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];

        if (target == null)
        {
            target = new GameObject(gameObject.name + " Target").transform;
            target.position = transform.position;
        }

        startRotationTarget = target.rotation;
        completeLength = 0;
        
        // Initialize data
        var current = transform;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = current;
            startRotationBone[i] = current.rotation;

            if (i == bones.Length - 1)
            {
                // Leaf
                startDirectionSucc[i] = target.position - current.position;
            }
            else
            {
                // Mid bone
                var midBonePos = current.position;
                startDirectionSucc[i] = bones[i + 1].position - midBonePos;
                bonesLength[i] = (bones[i + 1].position - midBonePos).magnitude;
                completeLength += bonesLength[i];
            }

            current = current.parent;
        }
    }

    private void ResolveIK()
    {
        if (target == null)
            return;

        if (bonesLength.Length != chainLength)
            Init();
        
        // Fabric
        // (bone0) (bone len 0) (bone1) (bone len 1) (bone2) ...
        //   x--------------------x---------------------x----...
        
        
        // Get positions
        for (int i = 0; i < bones.Length; i++)
            positions[i] = bones[i].position;

        var rootRotation = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        var rootRotationDiff = rootRotation * Quaternion.Inverse(startRotationRoot);
        
        // 1st is it possible to reach?
        if ((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
        {
            // Just stretch it
            var direction = (target.position - positions[0]).normalized;

            // Set everything after the root
            for (int i = 1; i < positions.Length; i++)
                positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
        }
        else
        {
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                // Back
                for (int i = positions.Length - 1; i > 0; i--)
                {
                    if (i == positions.Length - 1)
                        positions[i] = target.position; // Set it to target
                    else
                        positions[i] = positions[i + 1] + (positions[i] - positions[i + 1]).normalized * bonesLength[i]; // Set in line on distance
                }
                
                // Forward
                for (int i = 1; i < positions.Length; i++)
                    positions[i] = positions[i - 1] + (positions[i] - positions[i - 1]).normalized * bonesLength[i - 1];
                
                // Are we close enough? 
                if ((positions[positions.Length - 1] - target.position).sqrMagnitude < delta * delta)
                    break;

            }
        }
        
        // Move towards pole
        if (pole != null)
        {
            for (int i = 1; i < positions.Length - 1; i++)
            {
                var plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(pole.position);
                var projectedBone = plane.ClosestPointOnPlane(positions[i]);
                float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1],
                    plane.normal);
                positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) +
                    positions[i - 1];
            }
        }

        // Set positions && rotations
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * startRotationBone[i];
            }

            bones[i].position = positions[i];
        }
    }
}
