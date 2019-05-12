using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cactus : MonoBehaviour
{
    //When X reaches this value, cactus will destroy itself because it is not
    //Visible anymore
    public float destroyDist = -400;

    //A position which is used by dinos to calculate the distance between itself
    //and this cactus
    public Transform DistanceReference;

    //Manager object
    public AIManager manager;
    
    void Start()
    {
        //Get the AI manager
        manager = GameObject.Find("Manager").GetComponent<AIManager>();
    }

    void Update()
    {
        //Move left
        transform.Translate(-transform.right * manager.gameSpeed * Time.deltaTime);

        //If cactus is not visible anymore, destroy
        if (transform.position.x <= destroyDist)
            Destroy(gameObject);
    }
}
