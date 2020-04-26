using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    public MovementCycle currentCycle;
    public float moveSpeed;
    public GameObject poles;

    private bool moving = false;

    private void Awake()
    {
        currentCycle = MovementCycle.None;
    }

    private void FixedUpdate()
    {
        if((Input.GetAxis("Vertical") > 0))
        {
            poles.transform.position += (transform.forward * .5f) * (moveSpeed * Time.fixedDeltaTime);
        }
    }

    public void MoveBody(float movementTime)
    {
        var position = transform.position;
        var startPoint = position;
        var endPoint = position + (transform.forward * .5f);
        var control1 = position + (transform.up * .25f);
        var control2 = endPoint + (transform.up * .25f);
            
        moving = true;
        LeanTween.move(gameObject, new LTBezierPath(new Vector3[] {startPoint, control1, control2, endPoint}),
            movementTime).setEase(LeanTweenType.linear);
    }

    private void OnDrawGizmos()
    {
        var position = transform.position;
        var startPoint = position;
        var endPoint = position + (transform.forward * .5f);
        var control1 = position + (transform.up * .5f);
        var control2 = endPoint + (transform.up * .5f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(startPoint, .3f);
        Gizmos.DrawWireSphere(endPoint, .3f);
        Gizmos.DrawWireSphere(control1, .3f);
        Gizmos.DrawWireSphere(control2, .3f);
    }
}

public enum MovementCycle
{
    FirstCycle,
    SecondCycle,
    None
}
