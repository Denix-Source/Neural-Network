using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    public GameObject Top;
    public GameObject Bottom;

    // Random y position
    void Start()
    {
        transform.Translate(new Vector3(0, UnityEngine.Random.Range(-50, 50), 0));
    }

    // Always move to left
    void Update()
    {
        transform.Translate(-transform.right * 80 * Time.deltaTime);
    }
}
