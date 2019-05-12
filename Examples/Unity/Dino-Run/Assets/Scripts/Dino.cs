using UnityEngine;
using UnityEngine.UI;
using NeuralNet;

public class Dino : MonoBehaviour
{
    //Rigidbody2d component
    private Rigidbody2D rb;

    //Cactus queue which is used while calculating the distance to the nearest cactus
    public GameObject[] CactusQueue = new GameObject[0];

    //Used for calculating the distance to the nearest cactus
    public Transform DistanceReference;

    //Neural network used by this dino for calculating the output
    public NeuralNetwork net;

    //Sprites
    public Sprite Jumping;
    public Sprite Running1;
    public Sprite Running2;
    public Sprite Crashed;

    //Does dino touch the ground?
    public bool Ground = true;

    //Did dino crashed?
    public bool Stop = false;

    //Did dino registered that it crashed
    public bool AddedCountToManager = false;

    //How long this dino jumps
    public float JumpVelocity;

    //When dino crashes, it moves to the left. When it leaves the screen (when x <= stopDistance)
    // it stops moving
    public float StopDistance;

    //Fitness score
    public float Score = 0;

    void Start()
    {
        //Init variables
        rb = GetComponent<Rigidbody2D>();
        net = new NeuralNetwork(3, new int[2] { 2, 2 }, 1, Random.Range, ActivationFunctions.Step);
    }

    void Update()
    {
        //Used for updating the sprite
        float t = Time.time - (int)Time.time;
        if(Stop)
        {
            GetComponent<Image>().sprite = Crashed;
        }
        else if(!Ground)
        {
            GetComponent<Image>().sprite = Jumping;
        }
        else if (((int)(t / 0.200f)) % 2 == 0)
        {
            GetComponent<Image>().sprite = Running1;
        }
        else
        {
            GetComponent<Image>().sprite = Running2;
        }

        //If crahes, try moving left
        if(Stop)
        {
            //If dino is still visible, move left
            if(transform.position.x > StopDistance)
                transform.Translate(-transform.right * GameObject.Find("Manager").GetComponent<AIManager>().gameSpeed * Time.deltaTime);
        }
        else
        {
            //Is there is any cactus in the queue
            if(CactusQueue.Length > 0)
            {
                //Calculate the inputs

                //Distance to the next cactus
                float distance = CactusQueue[0].GetComponent<Cactus>().DistanceReference.position.x - DistanceReference.position.x;

                //Speed of the game
                float speed = GameObject.Find("Manager").GetComponent<AIManager>().gameSpeed;

                //Vertical velocity of the dino
                float velocity = rb.velocity.y;

                //Output calculated by the neural network
                float[] output = net.GetOutput(distance, speed, velocity);

                //If output is 1 (If you use step function it can be 1 or 0) try to jump
                if(output[0] == 1)
                {
                    if (Ground)
                    {
                        Ground = false;
                        rb.velocity = new Vector2(0, JumpVelocity);
                    }
                }
            }

            //Increase the fitness score if still not crashed
            Score += Time.deltaTime;
        }
    }


    //Called by the manager when new cactus is spawned
    public void AddCactusToQueue(GameObject cactus)
    {
        System.Array.Resize(ref CactusQueue, CactusQueue.Length + 1);
        CactusQueue[CactusQueue.Length - 1] = cactus;
    }

    //When dino touches an object
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If touches the ground dino can jump now
        if (collision.gameObject.tag == "Ground")
            Ground = true;

        //If touches a cactus, it crashes
        if(collision.gameObject.tag == "Cactus")
        {
            Stop = true;

            if(!AddedCountToManager)
            {
                //Increase manager's failed dino count variable by 1 if it didn't
                AddedCountToManager = true;
                GameObject.Find("Manager").GetComponent<AIManager>().failedDinoCount++;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    //If dino jumped over the cactus, start calculating the input by looking to the next cactus
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If there is 1 cactus left, clear the array. Remove the first element otherwise
        if(CactusQueue.Length > 1)
        {
            GameObject[] newQueue = new GameObject[CactusQueue.Length - 1];
            for(int i = 1;i < CactusQueue.Length;i++)
            {
                newQueue[i - 1] = CactusQueue[i];
            }
            CactusQueue = newQueue;
        }
        else
        {
            CactusQueue = new GameObject[0];
        }
    }
}
