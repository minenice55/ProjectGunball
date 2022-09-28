using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
    public float jumpForce = 5.0f;
    public bool isOnGround = true;
    private Rigidbody playerRb;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerRb.AddForce(Vector3.left);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerRb.AddForce(Vector3.right);
        }
        if (Input.GetKey(KeyCode.W)){
            playerRb.AddForce(Vector3.forward);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerRb.AddForce(Vector3.forward * -1);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
        }
    }
}
