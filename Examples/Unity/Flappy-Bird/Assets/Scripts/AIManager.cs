using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIManager : MonoBehaviour
{
    public GameObject[] pipes;

    float lastTime = 0;

    public GameObject pipe;
    public GameObject flappyB;

    public Transform canvas;
    public Transform pipeSpawn;

    public Text Generation;
    public Text HighScore;
    
    GameObject[] FlappyBirds;
    public int count = 0;

    public int gen;
    float highScore = 0;
    NeuralNet.NeuralNetwork bestNetwork;

    void Start()
    {
        //Init flappy birds
        pipes = new GameObject[1] { Instantiate(pipe, pipeSpawn) };
        FlappyBirds = new GameObject[50];
        for(int i = 0;i < 50;i++)
        {
            FlappyBirds[i] = Instantiate(flappyB, canvas);
            FlappyBirds[i].GetComponent<FlappyBird>().PipeQueue = pipes;
            FlappyBirds[i].GetComponent<FlappyBird>().Begin = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Remove destroyed pipes from arrays
        while(pipes[0] == null)
        {
            GameObject[] newArr = new GameObject[pipes.Length - 1];
            for(int i = 1;i < pipes.Length;i++)
            {
                newArr[i - 1] = pipes[i];
            }
            pipes = newArr;
        }
        //Spawn pipes every 2 seconds
        if(Time.time - lastTime >= 2)
        {
            Array.Resize(ref pipes, pipes.Length + 1);
            GameObject newPipe = Instantiate(pipe, pipeSpawn);
            pipes[pipes.Length - 1] = newPipe;
            lastTime = Time.time;

            for(int i = 0;i < FlappyBirds.Length;i++)
            {
                FlappyBirds[i].GetComponent<FlappyBird>().AddPipe(newPipe);
            }
        }
        //If all birds lost the game or key space pressed
        if(Input.GetKeyDown(KeyCode.Space) || count >= FlappyBirds.Length)
        {
            //Copy the neural network of the best flappy bird and destroy them
            for(int i = 0;i < FlappyBirds.Length;i++)
            {
                if(FlappyBirds[i].GetComponent<FlappyBird>().score > highScore)
                {
                    highScore = FlappyBirds[i].GetComponent<FlappyBird>().score;
                    bestNetwork = FlappyBirds[i].GetComponent<FlappyBird>().net;
                }
                Destroy(FlappyBirds[i].gameObject);
            }

            //Clear pipes
            for(int i = 0;i < pipes.Length;i++)
            {
                Destroy(pipes[i]);
            }

            //Reset last time so second pipe won't spawn late or early
            lastTime = Time.time;

            //Random neural net flappy birds x 9
            for(int i = 0;i < 9;i++)
            {
                FlappyBirds[i] = Instantiate(flappyB, canvas);
            }

            //Best neural network flappy birds (-1 to 1 change on weights and biases) x 20
            for(int i = 9;i < 29;i++)
            {
                FlappyBirds[i] = Instantiate(flappyB, canvas);
                FlappyBirds[i].GetComponent<FlappyBird>().net = new NeuralNet.NeuralNetwork(bestNetwork);
                FlappyBirds[i].GetComponent<FlappyBird>().net.AddRandomNumbersToWeights(-1, 1);
                FlappyBirds[i].GetComponent<FlappyBird>().net.AddRandomNumbersToBiases(-1, 1);
            }

            //Best neural network flappy birds (-0.1 to 0.1 change on weights and biases) x 20
            for (int i = 29; i < 49; i++)
            {
                FlappyBirds[i] = Instantiate(flappyB, canvas);
                FlappyBirds[i].GetComponent<FlappyBird>().net = new NeuralNet.NeuralNetwork(bestNetwork);
                FlappyBirds[i].GetComponent<FlappyBird>().net.AddRandomNumbersToWeights(-0.1f, 0.1f);
                FlappyBirds[i].GetComponent<FlappyBird>().net.AddRandomNumbersToBiases(-0.1f, 0.1f);
            }

            //Best neural network flappy bird x 1
            FlappyBirds[49] = Instantiate(flappyB, canvas);
            FlappyBirds[49].GetComponent<FlappyBird>().net = new NeuralNet.NeuralNetwork(bestNetwork);

            //Assign the first pipe and start the new generation
            pipes = new GameObject[1] { Instantiate(pipe, pipeSpawn) };
            for(int i = 0;i < FlappyBirds.Length;i++)
            {
                FlappyBirds[i].GetComponent<FlappyBird>().PipeQueue = pipes;
                FlappyBirds[i].GetComponent<FlappyBird>().Begin = true;
            }
            count = 0;
            Generation.text = "Generation : " + ++gen;
        }
    }

    //Update score counter on the screen
    public void Score(float score)
    {
        if(score > highScore)
            HighScore.text = "High Score : " + score;
    }
}
