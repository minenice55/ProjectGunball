using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPlate : MonoBehaviour
{
    public Vector3[] positions = new Vector3[9];
    public GameObject Sphere;
    public float min,max; 
    public float randomTime;
    // Start is called before the first frame update
    void Start()
    {
        positions[0] = new Vector3(3.5f,-1.2f,-1.3f);
        positions[1] = new Vector3(-0.05f,-6.4f,-0.95f);
        positions[2] = new Vector3(-0.05f,-2.5f,-42.5f);
        positions[3] = new Vector3(-0.05f,-2.5f,42.5f);
        positions[4] = new Vector3(-0.05f,-2.5f,68f);
        positions[5] = new Vector3(-0.05f,-2.5f,-68f);
        positions[6] = new Vector3(-32f,-2.5f,0.95f);
        positions[7] = new Vector3(32f,-2.5f,0.95f);
        positions[8] = new Vector3(21f,-9.2f,0.95f);
        positions[9] = new Vector3(-21f,-9.2f,0.95f);
    }

    // Update is called once per frame
    void Update()
    {
        randomTime = Random.Range(min,max);
        Invoke("SpawnPlate",randomTime);
    }

    IEnumerator SpawnPlate()
    {
        int place = Random.Range(0,9);
        Instantiate(Sphere, positions[place], Quaternion.Euler(new Vector3(0, 0, 0)));
        yield return new WaitForSeconds(10.0f);
        Destroy(Sphere);
    }
}
