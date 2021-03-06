﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] private float branchDetectionDistance = 50f;
    [SerializeField] private LayerMask branchesLayer;

    [SerializeField] private float minDelay;
    [SerializeField] private float maxDelay;

    [SerializeField] private GameObject projectile;
    [SerializeField] private float force;

    [SerializeField] private Transform target;
    [SerializeField] float initialAngle;

    void Start()
    {
        StartCoroutine(BehaviourLoop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private List<Branch> DetectNearBranches() {
        List<Branch> nearBranches = new List<Branch>();

        foreach(Collider col in Physics.OverlapSphere(this.transform.position, branchDetectionDistance, branchesLayer)){
            if (col.gameObject.GetComponent<Branch>() && col.gameObject.transform.position != this.transform.position) {

                Ray ray = new Ray(transform.position, col.gameObject.transform.position - transform.position);
                RaycastHit[] hits = Physics.RaycastAll(ray, branchDetectionDistance);

                bool possibleJump = true;

                foreach(RaycastHit hit in hits) {
                    if(hit.transform.gameObject.tag == "Tree") {
                        possibleJump = false;
                    }
                }
                if (possibleJump) { 
                    nearBranches.Add(col.gameObject.GetComponent<Branch>());
                }
            }
        }

        return nearBranches;
    }

    private IEnumerator BehaviourLoop() {
        while(true){
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if(Random.Range(0,2) == 0) {
                yield return Jump(DetectNearBranches());                
            }
            else {
                Shoot();
            }
        }
    }
    
    private void Shoot() {
        var currentProjectile = Instantiate(projectile, this.transform.position, Quaternion.identity);

        Vector3 direction = (GameObject.FindGameObjectWithTag("Player").transform.position - transform.position).normalized;

        currentProjectile.GetComponent<Rigidbody>().AddForce(direction*force, ForceMode.Impulse);
    }

    private IEnumerator Jump(List<Branch> branches) {
        StartCoroutine(JumpToBranch(branches[Random.Range(0, branches.Count)].transform));
        yield return null;
    }

    private IEnumerator JumpToBranch(Transform branch) {
        var rigid = GetComponent<Rigidbody>();

        Vector3 p = new Vector3(branch.position.x , (branch.position.y + branch.localScale.y) , branch.position.z);


        float gravity = Physics.gravity.magnitude;
        // Selected angle in radians
        float angle = initialAngle * Mathf.Deg2Rad;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(p.x, 0, p.z);
        Vector3 planarPostion = new Vector3(transform.position.x, 0, transform.position.z);

        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = transform.position.y - p.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));

        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (p.x > transform.position.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        // Fire!
        if (!float.IsNaN(finalVelocity.x) || !float.IsNaN(finalVelocity.y) || !float.IsNaN(finalVelocity.z)) {
            rigid.velocity = finalVelocity;
        }

        yield return null;
    }

    private void OnCollisionEnter(Collision collision) {
        var rigid = GetComponent<Rigidbody>();

        rigid.velocity = Vector3.zero;
    }

    private void OnDrawGizmos() {
        Gizmos.DrawWireSphere(this.transform.position, branchDetectionDistance);
    }
}
