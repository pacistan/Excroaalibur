using UnityEngine;

public class GBilboardBeahviour : MonoBehaviour
{
    public Camera cameraToLookAt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // cameraToLookAt = GetComponent<Camera>();
}

    // Update is called once per frame
    void Update()
    {
        Vector3 v = cameraToLookAt.transform.position - transform.position;
        v.x = v.z = 0.0f;
        transform.LookAt(cameraToLookAt.transform.position - v);
    }
}
