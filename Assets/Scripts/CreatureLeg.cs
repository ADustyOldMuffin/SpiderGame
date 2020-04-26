using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CreatureLeg : MonoBehaviour
{
    public CreatureMovementCycle movementCycle;
    public Transform boneTransform;
    public Transform target;
    public CreatureMovement creatureMovement;
    public float offset;
    
    public int index;
    public float distanceToTarget => Vector3.Distance(boneTransform.position, target.position);

    public LTDescr MoveLeg(float movementTime, float direction)
    {
        var startPoint = boneTransform.position;
        var endPoint = target.position + target.forward * (offset * direction);
        var controlPoint1 = boneTransform.position + (transform.up * .25f);
        var controlPoint2 = target.position + (target.up * .25f);

        return LeanTween.move(boneTransform.gameObject, new LTBezierPath(new []{startPoint, controlPoint1, controlPoint2, endPoint}), movementTime).setOnComplete(() => 
            { 
                creatureMovement.isGrounded[index] = true;
                creatureMovement.currentCycle = CreatureMovementCycle.None;
            });
    }
}
