using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNet;

public class FlappyBird : MonoBehaviour
{
    public GameObject[] PipeQueue;

    public float score = 0;
    public bool Stop = false;
    public bool Begin = false;
    public bool addedCount = false;

    public NeuralNet.NeuralNetwork net;

    // Start is called before the first frame update
    void Start()
    {
        // Init neural network which contains 2 inputs, 2 hidden layers
        // (2 neurons each) and 1 output. It also uses Unity's random
        // function and step function (every neuron's output value is
        // 1 or 0)
        net = new NeuralNet.NeuralNetwork(2, new int[2] {2, 2}, 1, Random.Range, ActivationFunctions.Step);
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the bird
        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(0, 0, (45 * GetComponent<Rigidbody2D>().velocity.y) / 250));
        if (!Stop && Begin)
        {
            //Calculate the inputs
            float b = transform.position.y - PipeQueue[0].GetComponent<Pipe>().Bottom.transform.position.y;
            float v = GetComponent<Rigidbody2D>().velocity.y;

            //Get the network output
            float[] output = net.GetOutput(b, v);

            //If first output neuron is bigger than the second one jump
            if(output[0] == 1)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(0, 250);
            }

            //Increase fitness score
            score += Time.deltaTime;

            //Update high score text
            GameObject.Find("Manager").GetComponent<AIManager>().Score(score);
        }
        else
        {
            //If lost, move to the left
            transform.Translate(-transform.right * 80 * Time.deltaTime);
        }
    }

    //Called by the manager. Adds a new pipe to the query
    public void AddPipe(GameObject pipe)
    {
        System.Array.Resize(ref PipeQueue, PipeQueue.Length + 1);
        PipeQueue[PipeQueue.Length - 1] = pipe;
    }

    //When leaves a pipe, next pipe will be used as an input
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject[] newArr = new GameObject[PipeQueue.Length - 1];
        for (int i = 1; i < PipeQueue.Length; i++)
        {
            newArr[i - 1] = PipeQueue[i];
        }
        PipeQueue = newArr;
    }

    //If hit to the ground or pipe, stop
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Block")
        {
            Stop = true;
            if(!addedCount)
            {
                GameObject.Find("Manager").GetComponent<AIManager>().count++;
                addedCount = true;
            }
        }
    }
}
