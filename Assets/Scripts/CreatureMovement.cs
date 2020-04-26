using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CreatureMovement : MonoBehaviour
{
    public GameObject[] legs;
    public GameObject[] targetsCastPoints;
    public bool[] isGrounded;
    public float moveLegDistance = .3f;
    public float heightOffset = .5f;
    public float legMovementTime = .5f;
    public float moveSpeed = 2;
    public float turnSpeed = 10;
    public bool debug = true;
    public CreatureMovementCycle currentCycle = CreatureMovementCycle.None;

    private GameObject[] targets;
    private bool isMoving = false;
    private int ignoreLayer;
    
    private void Awake()
    {
        ignoreLayer = ~(1<<8);
        targets = new GameObject[targetsCastPoints.Length];
        isGrounded = new bool[targetsCastPoints.Length];
        
        for (int i = 0; i < targetsCastPoints.Length; i++)
        {
            if (!Physics.Raycast(targetsCastPoints[i].transform.position, -targetsCastPoints[i].transform.up, out var hit, Mathf.Infinity, ignoreLayer))
                continue;

            var leg = legs[i].GetComponent<CreatureLeg>();
            
            var target = new GameObject($"{legs[i].name}_Target");
            targets[i] = target;
            target.transform.position = hit.point;
            leg.target = target.transform;
            leg.boneTransform.position = target.transform.position;
            leg.index = i;
            leg.creatureMovement = this;
            isGrounded[i] = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        transform.position += transform.forward * (moveSpeed * Time.deltaTime * verticalInput);
        transform.Rotate(new Vector3(0, 1, 0), horizontalInput * turnSpeed * Time.deltaTime, Space.World);

        float average = legs.Average(x => x.GetComponent<CreatureLeg>().boneTransform.position.y);
        var position = transform.position;
        position.y = average + heightOffset;
        transform.position = position;

        // Leg movement
        for (int i = 0; i < targetsCastPoints.Length; i++)
        {
            if (!Physics.Raycast(targetsCastPoints[i].transform.position, -targetsCastPoints[i].transform.up,
                out var hit, Mathf.Infinity, ignoreLayer))
            {
                continue;
            }
            
            if(hit.collider.gameObject.layer != 0)
                Debug.Log(hit.collider.gameObject.layer);

            if (debug)
            {
                Debug.DrawLine(targetsCastPoints[i].transform.position, hit.point, Color.red);
            }

            targets[i].transform.position = hit.point;

            var legComp = legs[i].GetComponent<CreatureLeg>();
            
            if ((legComp.movementCycle != currentCycle && currentCycle != CreatureMovementCycle.None) || !(legComp.distanceToTarget > moveLegDistance) || !isGrounded[i])
                continue;

            isGrounded[i] = false;
            currentCycle = legComp.movementCycle;

            legComp.MoveLeg(legMovementTime, verticalInput);
            
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward);
        
        if (!debug || !EditorApplication.isPlaying)
            return;
        
        foreach (var t in targets)
        {
            Gizmos.DrawWireSphere(t.transform.position, .2f);
        }
    }
}

public enum CreatureMovementCycle
{
    First,
    Second,
    None
}
