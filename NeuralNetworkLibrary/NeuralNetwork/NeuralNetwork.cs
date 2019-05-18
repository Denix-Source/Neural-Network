using System;

namespace NeuralNet
{
    /// <summary>
    /// Main neural network class. Can be inherited to create custom networks (such as backpropagation)
    /// </summary>
    public class NeuralNetwork
    {
        /********************************Variables********************************/
        #region VARIABLES
        
        //Protected variables which store actual values and used in properties
        #region PROPERTY_VARIABLES
        protected float[][][] _weights;
        protected float[][] _biases;
        protected RandomValue _randomValue;
        protected ActivationFunction _activationFunction;
        #endregion


        /// <summary>
        /// Determines if neural network is initialized
        /// </summary>
        protected bool Initialized = false;

        /// <summary>
        /// Length = number of layers. Elements = number of neurons in that layer
        /// </summary>
        protected int[] _layers;

        /// <summary>
        /// Weights of the neurons. Last layer doesn't have weight since it's the output layer
        /// </summary>
        public float[][][] Weights
        {
            get => _weights; //Returns the protected variable
            set
            {
                //If objects was not initialized using a constructor it will throw an exception
                if (!Initialized)
                    throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

                //If given value is null throw null reference exception
                if (value == null)
                    throw new NullReferenceException("Given variable is null");

                //Fill the array with the new array's values
                for (int x = 0; x < _weights.Length; x++)
                {
                    for (int y = 0; y < _weights[x].Length; y++)
                    {
                        for (int z = 0; z < _weights[x][y].Length; z++)
                        {
                            //If values are out of weights array, skip
                            if (x > value.Length - 1 || y > value[x].Length - 1 || z > value[x][y].Length)
                                continue;
                            
                            _weights[x][y][z] = value[x][y][z];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Biases of the neurons. Input layer doesn't have bias, so array begins from the first hidden layer
        /// </summary>
        public float[][] Biases
        {
            get => _biases; //Return the protected variable
            set
            {
                //If objects was not initialized using a constructor it will throw an exception
                if (!Initialized)
                    throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

                //If given value is null throw null reference exception
                if (value == null)
                    throw new NullReferenceException("Given variable is null");

                //Fill the array with the new array's values
                for (int x = 0; x < _biases.Length; x++)
                {
                    for (int y = 0; y < _biases[x].Length; y++)
                    {
                        //If values are out of biases array, skip
                        if (x > value.Length - 1 || y > value[x].Length)
                            continue;

                        _biases[x][y] = value[x][y];
                    }
                }
            }
        }

        /// <summary>
        /// Returns number of input neurons. If not initialized returns -1
        /// </summary>
        public int InputCount
        {
            //If initialized, return the first element of the layers array, else return -1
            get => (Initialized)? _layers[0] : -1;
        }

        /// <summary>
        /// Returns number of hidden layers. If not initialized returns -1
        /// </summary>
        public int HiddenLayerCount
        {
            //If initialized return the length of the layers array - 2, else -1
            get => (Initialized) ? _layers.Length - 2 : -1;
        }

        /// <summary>
        /// Returns number of neurons in hidden layers. If not initialized returns null
        /// </summary>
        public int[] HiddenNeurons
        {
            get
            {
                //If not initialized, return null
                if (!Initialized)
                    return null;

                //Array to return
                int[] output = new int[_layers.Length - 2];

                //Repeat length of the output array (all arrays - input array - output array = all arrays - 2)
                for(int i = 0;i < output.Length;i++)
                {
                    //Ignore the input array by adding 1 to i
                    output[i] = _layers[i + 1];
                }

                return output;
            }
        }

        /// <summary>
        /// Returns the number of output neurons. If not initialized returns -1
        /// </summary>
        public int OutputCount
        {
            //If initialized return the last element of the layers array, else -1
            get => (Initialized)? _layers[_layers.Length - 1] : -1;
        }

        /// <summary>
        /// Function used to generate random numbers
        /// </summary>
        public RandomValue randomValue
        {
            //Return the protected variable
            get => _randomValue;
            set
            {
                //if not null set the protected variable to given value, else throw null reference excepciton
                _randomValue = value ?? throw new NullReferenceException("Given variable is null");
            }
        }

        /// <summary>
        /// Function used to calculate the output of neurons
        /// </summary>
        public ActivationFunction activationFunction
        {
            //Return the protected variable
            get => _activationFunction;
            //Set protected variable to given value if not null, else throw null reference exception
            set => _activationFunction = value ?? throw new NullReferenceException("Given variable is null");
        }

        //Delegates which are used for functions and generating random values
        /// <summary>
        /// Function used to generate random numbers in range of given minimum and maximum values
        /// </summary>
        /// <param name="minValue">Minimum value function can return</param>
        /// <param name="maxValue">Maximum value function can return</param>
        /// <returns>Random value</returns>
        public delegate float RandomValue(float minValue, float maxValue);

        /// <summary>
        /// Function used for calculating the output of neurons
        /// </summary>
        /// <param name="input">Output of the neuron</param>
        public delegate float ActivationFunction(float input);

        //Default random generator variable
        private Random _random;
        #endregion

        /******************************Constructers*******************************/
        #region CONSTRUCTORS
        //All consturcors use Init method. If bounds of weights/biases are not given weights will
        //be between -10 and 10, biases will be between -1 and 1. If no custom functions are given (null),
        //default functions will be used. Arrays will be assigned to weights/biases if they are given

        //IMPORTANT: If you create a custom neural network, DO NOT forget to add Initialized = true; to the constructors
        /// <summary>
        /// Basic neural network constructor
        /// </summary>
        /// <param name="inputCount">Number of input neurons</param>
        /// <param name="hiddenLayerSize">Number of hidden layers. Length = number of hidden layers, Elements = number of neurons in that layer</param>
        /// <param name="outputCount">Number of output neurons</param>
        public NeuralNetwork(int inputCount, int[] hiddenLayerSize, int outputCount)
        {
            Init(inputCount, hiddenLayerSize, outputCount, -10f, 10f, -1f, 1f, null, null, null, null);
        }

        /// <summary>
        /// Basic neural network constructor with custom functions. Assign function as null for default function
        /// </summary>
        /// <param name="inputCount">Number of input neurons</param>
        /// <param name="hiddenLayerSize">Number of hidden layers. Length = number of hidden layers, Elements = number of neurons in that layer</param>
        /// <param name="outputCount">Number of output neurons</param>
        /// <param name="randomVal">Custom random value generator. Assign as null to use default System.Random generator</param>
        /// <param name="activationFunc">Custom activation function. Assign as null to use default linear function (returns the input)</param>
        public NeuralNetwork(int inputCount, int[] hiddenLayerSize, int outputCount, RandomValue randomVal = null, ActivationFunction activationFunc = null)
        {
            Init(inputCount, hiddenLayerSize, outputCount, -10f, 10f, -1f, 1f, null, null, randomVal, activationFunc);
        }

        /// <summary>
        /// Advanced neural network constructor
        /// </summary>
        /// <param name="inputCount">Number of input neurons</param>
        /// <param name="hiddenLayerSize">Number of hidden layers. Length = number of hidden layers, Elements = number of neurons in that layer</param>
        /// <param name="outputCount">Number of output neurons</param>
        /// <param name="minWeight">Minimum value a weight can take</param>
        /// <param name="maxWeight">Maximum value a weight can take</param>
        /// <param name="minBias">Minimum value a bias can take</param>
        /// <param name="maxBias">Maximum value a bias can take</param>
        /// <param name="randomVal">Custom random value generator. Assign as null to use default System.Random generator</param>
        /// <param name="activationFunc">Custom activation function. Assign as null to use default linear function (returns the input)</param>
        public NeuralNetwork(int inputCount, int[] hiddenLayerSize, int outputCount, float minWeight, float maxWeight, float minBias, float maxBias, RandomValue randomVal = null, ActivationFunction activationFunc = null)
        {
            Init(inputCount, hiddenLayerSize, outputCount, minWeight, maxWeight, minBias, maxBias, null, null, randomVal, activationFunc);
        }

        /// <summary>
        /// Advanced neural network constructor
        /// </summary>
        /// <param name="inputCount">Number of input neurons</param>
        /// <param name="hiddenLayerSize">Number of hidden layers. Length = number of hidden layers, Elements = number of neurons in that layer</param>
        /// <param name="outputCount">Number of output neurons</param>
        /// <param name="minWeight">Minimum value a weight can take. If weights parameter is defined, that array will be assigned to the weights</param>
        /// <param name="maxWeight">Maximum value a weight can take. If weights parameter is defined, that array will be assigned to the weights</param>
        /// <param name="minBias">Minimum value a bias can take. If biases parameter is defined, that array will be assigned to the biases</param>
        /// <param name="maxBias">Maximum value a bias can take. If biases parameter is defined, that array will be assigned to the biases</param>
        /// <param name="weights">If not null this array will be assigned to the neural network's weights array</param>
        /// <param name="biases">If not null this array will be assigned to the neural network's bias array</param>
        /// <param name="randomVal">Custom random value generator. Assign as null to use default System.Random generator</param>
        /// <param name="activationFunc">Custom activation function. Assign as null to use default linear function (returns the input)</param>
        public NeuralNetwork(int inputCount, int[] hiddenLayerSize, int outputCount, float minWeight, float maxWeight, float minBias, float maxBias, float[][][] weights = null, float[][] biases = null, RandomValue randomVal = null, ActivationFunction activationFunc = null)
        {
            Init(inputCount, hiddenLayerSize, outputCount, minWeight, maxWeight, minBias, maxBias, weights, biases, randomVal, activationFunc);
        }

        /// <summary>
        /// Creates the instance of the given neural network
        /// </summary>
        /// <param name="instance">Original neural network</param>
        public NeuralNetwork(NeuralNetwork instance)
        {
            if (instance == null)
                throw new NullReferenceException("Neural network was null");
            if (!instance.Initialized)
                throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

            _layers = instance._layers;
            _weights = instance._weights;
            _biases = instance._biases;
            _randomValue = instance._randomValue;
            _activationFunction = instance._activationFunction;

            Initialized = true;
        }
        #endregion

        /*********************************Methods*********************************/
        #region PRIVATE-METHODS
        //Used for initializing the object
        private void Init(int inputCount, int[] hiddenLayerSize, int outputCount, float minWeight, float maxWeight, float minBias, float maxBias, float[][][] weights, float[][] biases, RandomValue randomVal, ActivationFunction activationFunc)
        {
            //Set the Initialized variable to true
            Initialized = true;

            //Init functions. Assign default functions if they are null
            if (randomVal != null)
                _randomValue = randomVal;
            else
            {
                _random = new Random();
                _randomValue = DefaultRandomValue;
            }

            if (activationFunc != null)
                _activationFunction = activationFunc;
            else
                _activationFunction = DefaultActivationFunction;
            

            //Layer length = Input layer + Hidden layer count + Output layer 
            // = Hidden layer count + 2
            _layers = new int[hiddenLayerSize.Length + 2];

            //First element is count of input neurons
            _layers[0] = inputCount;

            //Last element is count of output layers
            _layers[_layers.Length - 1] = outputCount;

            //Elements between first and last elements are count of hidden layer neurons
            for (int i = 0; i < hiddenLayerSize.Length; i++)
                _layers[i + 1] = hiddenLayerSize[i];


            // Init weights. Output layer doesn't have any weights, so size of the weights
            //array is count of all layers - 1
            _weights = new float[_layers.Length - 1][][];

            //If given custom array is not null, use it's values
            if (weights != null)
            {
                for(int x = 0;x < _weights.Length;x++)
                {
                    //Length of that element equals to the count of neurons in that layer
                    _weights[x] = new float[_layers[x]][];
                    for (int y = 0; y < _weights[x].Length; y++)
                    {
                        //Length of that element equals to the count of the next layer. These are the weights connected to the next neurons
                        _weights[x][y] = new float[_layers[x + 1]];
                        for (int z = 0; z < _weights[x][y].Length; z++)
                        {
                            //If variables are out of bounds skip
                            if (x > weights.Length - 1 || y > weights[x].Length - 1 || z > weights[x][y].Length)
                                continue;
                            
                            //Set that element to the custom array's element which is at the same position
                            _weights[x][y][z] = weights[x][y][z];
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < _weights.Length; x++)
                {
                    //Length of that element equals to the count of neurons in that layer
                    _weights[x] = new float[_layers[x]][];
                    for (int y = 0; y < _weights[x].Length; y++)
                    {
                        //Length of that element equals to the count of the next layer. These are the weights connected to the next neurons
                        _weights[x][y] = new float[_layers[x + 1]];

                        //Set weights to the random values between the given bounds (default: -10 to 10)
                        for (int z = 0; z < _weights[x][y].Length; z++)
                            _weights[x][y][z] = _randomValue.Invoke(minWeight, maxWeight); //Invoke random value generator variable
                    }
                }
            }

            //Init biases. Input neurons don't have biases, so length of the array equals to
            //count of hidden layers + count of output layers
            // = count of all layers - 1
            _biases = new float[_layers.Length - 1][];

            //If custom array is not null use it's values
            if(biases != null)
            {
                for (int x = 0; x < _biases.Length; x++)
                {
                    //This element equals to the count of neurons in the next layer (skip the input layer)
                    _biases[x] = new float[_layers[x + 1]];
                    for (int y = 0; y < _biases[x].Length; y++)
                    {
                        //If values are out of bounds, skip
                        if (x > _biases.Length - 1 || y > biases[x].Length)
                            continue;

                        //Set that element to the custom array's element which is at the same position
                        _biases[x][y] = biases[x][y];
                    }
                }
            }
            else
            {
                for(int x = 0;x < _biases.Length;x++)
                {
                    //This element equals to the count of neurons in the next layer (skip the input layer)
                    _biases[x] = new float[_layers[x + 1]];

                    //Set the biases to random values (default: -1 to 1)
                    for(int y = 0;y < _biases[x].Length;y++)
                        _biases[x][y] = _randomValue.Invoke(minBias, maxBias);
                }
            }
        }

        //Default random value generator
        private float DefaultRandomValue(float min, float max)
        {
            float random = ((float)_random.NextDouble() - 0.5f) * 2;

            if (random < 0)
                random *= (min * -1);
            else
                random *= max;

            return random;
        }

        //Default activation function (linear)
        private float DefaultActivationFunction(float input) => input;
        #endregion
        #region PUBLIC-METHODS
        /// <summary>
        /// Get the output from the neural network
        /// </summary>
        /// <param name="inputs">Values which will be assigned to the input neurons</param>
        /// <returns>Values of the output layers</returns>
        public virtual float[] GetOutput(params float[] inputs)
        {
            //If inputs array is null, throw null reference exception
            if (inputs == null)
                throw new NullReferenceException("Input is null");

            //If object was not initialized using a constructor, throw exception
            if (!Initialized)
                throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

            //If there aren't enough inputs, throw exception (ex: 3 inputs were given but count of input neurons is 5)
            if (inputs.Length < _layers[0])
                throw new Exceptions.InvalidInputException(inputs.Length, _layers[0]);


            //Temp array where values of the neurons will be stored
            float[][] neurons = new float[_layers.Length][];

            //Set the count of neurons in the layers
            for(int i = 0;i < neurons.Length;i++)
            {
                neurons[i] = new float[_layers[i]];
            }

            //Assign the inputs to the neurons array
            for(int i = 0;i < neurons[0].Length;i++)
            {
                neurons[0][i] = inputs[i];
            }
            
            for(int x = 0;x < _weights.Length;x++)
            {
                for(int y = 0;y < _weights[x].Length;y++)
                {
                    for(int z = 0;z < _weights[x][y].Length;z++)
                    {
                        //Next neuron equals to the weighted sum of the last neurons, so add these values
                        //to that neuron. For loop starts from the input layer and ends at the last hidden
                        //layer. Every neuron's value is multiplied with it's weight and added to the next neuron
                        neurons[x + 1][z] += neurons[x][y] * _weights[x][y][z];
                    }
                }
                for(int y = 0;y < neurons[x + 1].Length;y++)
                {
                    //Add the bias to the neuron
                    neurons[x + 1][y] += _biases[x][y];
                    //Set the output of the neuron using the activation function
                    neurons[x + 1][y] = _activationFunction.Invoke(neurons[x + 1][y]);
                }
            }

            //Return the output layers
            return neurons[neurons.Length - 1];
        }

        /// <summary>
        /// Adds random numbers to weights
        /// </summary>
        /// <param name="minimumValue">Minimum value that can be added to a weight</param>
        /// <param name="maximumValue">Maximum value that can be added to a weight</param>
        public virtual void AddRandomNumbersToWeights(float minimumValue, float maximumValue)
        {
            //If object was not initialized using constructors, throw null reference exception
            if (!Initialized)
                throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

            for(int x = 0;x < _weights.Length;x++)
            {
                for(int y = 0;y < _weights[x].Length;y++)
                {
                    for(int z = 0;z < _weights[x][y].Length;z++)
                    {
                        //Add random values to the weights
                        _weights[x][y][z] += _randomValue.Invoke(minimumValue, maximumValue);
                    }
                }
            }
        }

        /// <summary>
        /// Adds random numbers to biases
        /// </summary>
        /// <param name="minimumValue">Minimum value that can be added to a bias</param>
        /// <param name="maximumValue">Maximum value that can be added to a bias</param>
        public virtual void AddRandomNumbersToBiases(float minimumValue, float maximumValue)
        {
            //If object was not initialized using constructors, throw null reference exception
            if (!Initialized)
                throw new Exceptions.NonInitializedNeuralNetworkException("Neural network was not initialized");

            for (int x = 0; x < _biases.Length; x++)
            {
                for (int y = 0; y < _biases[x].Length; y++)
                {
                    //Add random values to the biases
                    _biases[x][y] += _randomValue.Invoke(minimumValue, maximumValue);
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Most used activation functions in neural networks
    /// </summary>
    public static class ActivationFunctions
    {
        /// <summary>
        /// Returns 1 if value is bigger than 0, returns 0 otherwise
        /// </summary>
        /// <returns>1 or 0</returns>
        public static float Step(float input) => (input > 0) ? 1 : 0; //Return 1 if input is bigger than 0, else return 0

        /// <summary>
        /// Returns the value
        /// </summary>
        /// <returns>Given value (between -inf and +inf)</returns>
        public static float Linear(float input) => input; //Return the input

        /// <summary>
        /// Sigmoid function's output is in range between 0 and 1
        /// </summary>
        /// <returns>Value between 0 and 1</returns>
        public static float Sigmoid(float input) => 1 / (1 + (float)Math.Exp(-input)); //Return a value calculated using sigmoid function. Output is between 0 and 1

        /// <summary>
        /// Tahn function's output is in range between -1 and 1
        /// </summary>
        /// <returns>Value between -1 and 1</returns>
        public static float Tanh(float input) => (2 / (1 + (float)Math.Exp(-2 * input))) - 1; //Return a value calculated using tanh function. Output is between -1 and 1

        /// <summary>
        /// Returns 0 if value is negative or 0, value otherwise
        /// </summary>
        /// <returns>Value between 0 and +inf</returns>
        public static float ReLu(float input) => Math.Max(input, 0); //If input is bigger than 0 return the input, else return 0
    }

    /// <summary>
    /// Custom exceptions for NeuralNetwork class
    /// </summary>
    namespace Exceptions
    {
        /// <summary>
        /// Thrown when a method was called in a non-initialized network
        /// </summary>
        public class NonInitializedNeuralNetworkException : Exception
        {
            public NonInitializedNeuralNetworkException() : base("Neural network was not initialized") { } //Use the normal message if it was not given
            public NonInitializedNeuralNetworkException(string message) : base(message) { } //Use the given message
        }

        /// <summary>
        /// Thrown when number of given inputs are smaller than number of required inputs
        /// </summary>
        public class InvalidInputException : Exception
        {
            //Count inputs given (length of the inputs array)
            public int GivenInputLength = 0;
            //Count of input neurons
            public int RequiredInputLength = 0;

            //Call the base method and set the message using given values
            public InvalidInputException(int givenInputLength, int requiredInputLength) : base("Length of inputs (" + givenInputLength.ToString() + ") were smaller than required input count (" + requiredInputLength.ToString() + ")")
            {
                //Set the variables
                GivenInputLength = givenInputLength;
                RequiredInputLength = requiredInputLength;
            }
        }
    }
}