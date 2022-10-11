using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpDrop : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] Transform hold;
    private GameObject held;
    private Rigidbody heldObj;

    [Header("Physics Parameters")]
    [SerializeField] private float pickup = 5.0f;
    [SerializeField] private float force = 150.0f;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {// .GetMouseButtonDown(0)) {
            if (held == null) {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickup)) {
                    PickupObject(hit.transform.gameObject);
                }
            }

            else {
                DropObject();
            }
        }

        if (held != null) {
            MoveObject();
        }

    }

    void MoveObject() {
        if (Vector3.Distance(held.transform.position, hold.position) > 0.1f) {
            Vector3 moveDirection = (hold.position - held.transform.position);
            heldObj.AddForce(moveDirection * force);
        }
    }

    void PickupObject(GameObject pickObj) {
        if(pickObj.GetComponent<Rigidbody>()) {
            heldObj = pickObj.GetComponent<Rigidbody>();
            heldObj.useGravity = false;
            heldObj.drag = 10;
            heldObj.constraints = RigidbodyConstraints.FreezeRotation;

            heldObj.transform.parent = hold;
            held = pickObj;
        }
    }

    void DropObject() {
        heldObj.useGravity = true;
        heldObj.drag = 1;
        heldObj.constraints = RigidbodyConstraints.None;

        held.transform.parent = null;
        held = null;
    }
}