using UnityEngine;
using UnityEngine.UI;
using NeuralNet;

public class AIManager : MonoBehaviour
{
    //Dino prefab
    public GameObject DinoPrefab;

    //Cactus prefabs
    public GameObject[] Cactuses;

    //Array which contains cactuses visible on the screen
    public GameObject[] Queue;

    //All dino instantiates
    public GameObject[] Dinos;

    //Text which shows the generation information
    public Text GenText;

    //Text which shows highest score made by the best dino
    public Text HiScoreText;


    //Transform which will be the parent of dinos
    public Transform dinoSpawn;

    //Transform which will be the parent of cactuses
    public Transform cactusSpawn;

    //Neural network with the highest fitness score
    public NeuralNetwork bestNet;


    //Number of cactuses spawned. Every 5 cactuses this counter will be set
    //to 0 and speed will be increased
    public int spawnCount = 0;

    //How fast cactuses are
    public float gameSpeed = 160;

    //Highest score made by the best dino
    public float HighScore = 0;


    //How many dinos failed in the current generation
    public int failedDinoCount = 0;

    //Current generation
    public int Generation = 0;

    //Current score made by current generation's dinos
    public float currentScore = 0;
    
    void Start()
    {
        //Init the first generation
        Queue = new GameObject[1] {Instantiate(Cactuses[Random.Range(0, Cactuses.Length)], cactusSpawn) };
        Dinos = new GameObject[51];
        for(int i = 0;i < Dinos.Length;i++)
        {
            Dinos[i] = Instantiate(DinoPrefab, dinoSpawn);
            Dinos[i].GetComponent<Dino>().CactusQueue = Queue;
        }
    }
    
    void Update()
    {
        //Add delta time to the currentScore, so it equals to that generation's current score
        currentScore += Time.deltaTime;

        //If current generation's score is higher than highest score made, update the high
        //score variable and text
        if(currentScore > HighScore)
        {
            HighScore = currentScore;
            HiScoreText.text = "High score: " + HighScore;
        }

        //If there is any cactus, check if any of them were destroyed
        if (Queue.Length > 0)
        {
            //Repeat untill all of the destroyed cactuses are removed from the array
            while(Queue[0] == null)
            {
                GameObject[] newArray = new GameObject[Queue.Length - 1];
                for (int i = 1; i < Queue.Length; i++)
                {
                    //Copy every element in the Queue array except the null cactus
                    newArray[i - 1] = Queue[i];
                }

                //Update the array
                Queue = newArray;
            }
        }

        //If lastest cactus' x is equals/smaller than 21 spawn new one
        if(Queue[Queue.Length - 1].transform.position.x < 21)
        {
            //A game speed will increase in every 5 cactus
            if (++spawnCount >= 5)
            {
                //Reset the counter
                spawnCount = 0;

                //If game speed is faster than 420, it will be impossible to play
                if(gameSpeed < 420)
                    gameSpeed += 20;
            }

            //Increase the array's size by 1 to put the new instant to the last element
            System.Array.Resize(ref Queue, Queue.Length + 1);

            //Random cactus
            Queue[Queue.Length - 1] = Instantiate(Cactuses[Random.Range(0, Cactuses.Length)], cactusSpawn);

            //Add this cactus to every dino's queue
            for(int i = 0;i < Dinos.Length;i++)
            {
                Dinos[i].GetComponent<Dino>().AddCactusToQueue(Queue[Queue.Length - 1]);
            }
        }

        //If all dinos failed, start the new generation
        if(failedDinoCount >= Dinos.Length)
        {
            //Update genertaion texts
            Generation += 1;
            GenText.text = "Generation " + Generation;

            //Reset the variables
            currentScore = 0;
            failedDinoCount = 0;
            gameSpeed = 160;

            //Check if any dino made a better score than the best one made before
            for (int i = 0; i < Dinos.Length; i++)
            {
                if(Dinos[i].GetComponent<Dino>().Score >= HighScore)
                {
                    HighScore = Dinos[i].GetComponent<Dino>().Score;
                    bestNet = Dinos[i].GetComponent<Dino>().net;
                }
                //Destroy every dino to spawn new generation's
                Destroy(Dinos[i]);
            }

            //Destroy the cactuses left from the old generation
            for(int i = 0;i < Queue.Length;i++)
            {
                if (Queue[i] != null)
                    Destroy(Queue[i]);
            }
            //Init the first cactus
            Queue = new GameObject[1] { Instantiate(Cactuses[Random.Range(0, Cactuses.Length)], cactusSpawn) };


            //10 dinos start with random weights/biases
            for (int i = 0; i < 10; i++)
            {
                Dinos[i] = Instantiate(DinoPrefab, dinoSpawn);
                Dinos[i].GetComponent<Dino>().CactusQueue = Queue;
            }

            //20 dinos start with best algorythm but their weights and biases will be
            //increased by a value between -1 and 1
            for (int i = 10; i < 30; i++)
            {
                Dinos[i] = Instantiate(DinoPrefab, dinoSpawn);
                Dinos[i].GetComponent<Dino>().CactusQueue = Queue;
                Dinos[i].GetComponent<Dino>().net = new NeuralNetwork(bestNet);
                Dinos[i].GetComponent<Dino>().net.AddRandomNumbersToBiases(-1, 1);
                Dinos[i].GetComponent<Dino>().net.AddRandomNumbersToWeights(-1, 1);
            }

            //20 dinos start with best algorythm but their weights and biases will be
            //increased by a value between -0.1 and 0.1
            for (int i = 30; i < 50; i++)
            {
                Dinos[i] = Instantiate(DinoPrefab, dinoSpawn);
                Dinos[i].GetComponent<Dino>().CactusQueue = Queue;
                Dinos[i].GetComponent<Dino>().net = new NeuralNetwork(bestNet);
                Dinos[i].GetComponent<Dino>().net.AddRandomNumbersToBiases(-0.1f, 0.1f);
                Dinos[i].GetComponent<Dino>().net.AddRandomNumbersToWeights(-0.1f, 0.1f);
            }

            //1 dino starts with the best algorythm with no changes on weights & biases
            Dinos[50] = Instantiate(DinoPrefab, dinoSpawn);
            Dinos[50].GetComponent<Dino>().CactusQueue = Queue;
            Dinos[50].GetComponent<Dino>().net = new NeuralNetwork(bestNet);
        }
    }
}
