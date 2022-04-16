using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

namespace Completed
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class PlayerAgent : Agent
    {
        private Player player;
        private int lastAction = 0;

        [SerializeField] private GameManager gameManager;

        private float playerFood;
        private float foodValue = 0.02f;
        //private float foodLostPenalty = -0.5f;

        private Node exitNode;
        private Node playerNode;
        private List<Vector2> listOfFoodVectors;
        private List<Vector2> listOfSodaVectors;
        private List<Vector2> listOfInnerWallVectors;
        private List<Vector2> listOfEnemyVectors;
        private Vector2 playerToExitVector2;

        private int maxWallCount = 9;
        private int maxFoodCount = 5;
        private int maxEnemyCount = 6;

        void Start()
        {
            player = GetComponent<Player>();
        }

        public override void OnEpisodeBegin()
        {
            playerNode = gameManager.gridState.GetPlayerNode();
            exitNode = gameManager.gridState.GetExitNode();
            UpdateObservations();
        }

        public void HandleAttemptMove()
        {
            // TODO: Change the reward below as appropriate. If you want to add a cost per move, you could change the reward to -1.0f (for example).
            AddReward(0.0f);
        }

        public void HandleFinishlevel()
        {
            AddReward(playerFood);
        }

        public void HandleFoundFood()
        {
            AddReward(0.0f);
        }

        public void HandleFoundSoda()
        {
            //AddReward(0.5f);
            AddReward(0.0f);
        }

        public void HandleLoseFood(int loss)
        {
            //AddReward(-loss * foodValue); // -0.5f
            AddReward(-0.5f);
        }

        public void HandleLevelRestart(bool gameOver)
        {
            if (gameOver)
            {
                Debug.Log("Level Reached" + gameManager.level);
                //Academy.Instance.StatsRecorder.Add("Level Reached", GameManager.instance.level);
                Academy.Instance.StatsRecorder.Add("Level Reached", gameManager.level);
                EndEpisode();
            }

            // NOTE: You might also want to end the episode whenever the player successfully reaches the exit sign. You can achieve this by uncommenting the below:

            else
            {
                // Probably *is* best to consider episodes finished when the exit is reached
                EndEpisode();
            }

        }

        public override void CollectObservations(VectorSensor sensor)
        {
            UpdateObservations();

            sensor.AddObservation(playerToExitVector2);
            sensor.AddObservation(playerFood);

            // add wall to sensor
            for (int i = 0; i < maxWallCount; i++)
            {
                if(i < listOfInnerWallVectors.Count)
                {
                    sensor.AddObservation(listOfInnerWallVectors[i]);
                }
                else
                {
                    sensor.AddObservation(new Vector2(0, 0));
                }
            }

            // add food to sensor
            for (int i = 0; i < maxFoodCount; i++)
            {
                if (i < listOfFoodVectors.Count)
                {
                    sensor.AddObservation(listOfFoodVectors[i]);
                }
                else
                {
                    sensor.AddObservation(new Vector2(0, 0));
                }
            }

            // add soda to sensor
            // uses the same max food count
            for (int i = 0; i < maxFoodCount; i++)
            {
                if (i < listOfSodaVectors.Count)
                {
                    sensor.AddObservation(listOfSodaVectors[i]);
                }
                else
                {
                    sensor.AddObservation(new Vector2(0, 0));
                }
            }

            // add enemies to sensor
            for (int i = 0; i < maxEnemyCount; i++)
            {
                if (i < listOfEnemyVectors.Count)
                {
                    sensor.AddObservation(listOfEnemyVectors[i]);
                }
                else
                {
                    sensor.AddObservation(new Vector2(0, 0));
                }
            }

            base.CollectObservations(sensor);
        }

        private void UpdateObservations()
        {
            playerFood = player.food / 100.0f;
            playerNode = gameManager.gridState.GetPlayerNode();
            exitNode = gameManager.gridState.GetExitNode();
            playerToExitVector2 = ConvertNodeToVector2(exitNode);

            // get list of all inner walls in the scene (5-9 walls)
            List<Node> listofInnerWalls = gameManager.gridState.GetListOfNodesWithInnerWalls();

            // sort by distance to the player
            listofInnerWalls.Sort(SortByDistanceToPlayer);
            listOfInnerWallVectors = new List<Vector2>();

            // convert to normalised values that make more sense to the agent using formulas from lecture
            foreach (Node node in listofInnerWalls)
            {
                listOfInnerWallVectors.Add(ConvertNodeToVector2(node));
            }


            // get list of all food objects in the scene (1-5 walls)
            List<Node> listOfFoodObjects = gameManager.gridState.GetListOfNodesWithFood();

            // sort by distance to the player
            listOfFoodObjects.Sort(SortByDistanceToPlayer);
            listOfFoodVectors = new List<Vector2>();

            // convert to normalised values that make more sense to the agent using formulas from lecture
            foreach (Node node in listOfFoodObjects)
            {
                listOfFoodVectors.Add(ConvertNodeToVector2(node));
            }


            // get list of all food objects in the scene (1-5 walls)
            List<Node> listOfSodaObjects = gameManager.gridState.GetListOfNodesWithSoda();

            // sort by distance to the player
            listOfSodaObjects.Sort(SortByDistanceToPlayer);
            listOfSodaVectors = new List<Vector2>();

            // convert to normalised values that make more sense to the agent using formulas from lecture
            foreach (Node node in listOfSodaObjects)
            {
                listOfSodaVectors.Add(ConvertNodeToVector2(node));
            }


            // get list of all enemies in the scene up to level 30 logbase2(30) = 4.9 (1-5)
            List<Node> listOfEnemyObjeccts = gameManager.gridState.GetListOfNodesWithEnemies();

            // sort by distance to the player
            listOfEnemyObjeccts.Sort(SortByDistanceToPlayer);
            listOfEnemyVectors = new List<Vector2>();

            // convert to normalised values that make more sense to the agent using formulas from lecture
            foreach (Node node in listOfEnemyObjeccts)
            {
                listOfEnemyVectors.Add(ConvertNodeToVector2(node));
            }
        }

        private int SortByDistanceToPlayer(Node a, Node b)
        {
            float squaredRangeA = (new Vector2(a.row, a.column) - new Vector2(playerNode.row, playerNode.column)).sqrMagnitude;
            float squaredRangeB = (new Vector2(b.row, b.column) - new Vector2(playerNode.row, playerNode.column)).sqrMagnitude;
            return squaredRangeA.CompareTo(squaredRangeB);
        }

        private Vector2 ConvertNodeToVector2(Node nodeToConvert)
        {
            float playerToNodeToConvertX = nodeToConvert.row - playerNode.row;
            float playerToNodeToConvertY = nodeToConvert.column - playerNode.column;

            float distance = Mathf.Sqrt(playerToNodeToConvertX * playerToNodeToConvertX + playerToNodeToConvertY * playerToNodeToConvertY);
            
            //prevents NaN float values
            if(distance == 0)
            {
                return (new Vector2(0, 0));
            }

            float xDerivative = 1 / distance * (playerToNodeToConvertX);
            float yDerivative = 1 / distance * (playerToNodeToConvertY);

            float k = 0.1f;

            float exponentialX = Mathf.Exp(-k * distance) * xDerivative;
            float exponentialY = Mathf.Exp(-k * distance) * yDerivative;

            Vector2 returnVector2 = new Vector2(exponentialX, exponentialY);
            return returnVector2;
        }

        private bool CanMove()
        {
            //return !(player.isMoving || player.levelFinished || player.gameOver || GameManager.instance.doingSetup);
            return !(player.isMoving || player.levelFinished || player.gameOver || gameManager.doingSetup);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            //If it's not the player's turn, exit the function.
            //if (!CanMove()) return;
            if (gameManager.playerMovesSinceEnemyMove == gameManager.playerMovesPerEnemyMove && CanMove() && gameManager.playerMoving == false)
            {
                return;
            }

            gameManager.playerTurn = false;

            lastAction = (int)actions.DiscreteActions[0] + 1; // To allow standing still as an action, remove the +1 and change "Branch 0 size" to 5.

            switch (lastAction)
            {
                case 0:
                    break;
                case 1:
                    //player.AttemptMove<Wall>(-1, 0);
                    gameManager.playerMoving = true;
                    player.AttemptMove(-1, 0);
                    break;
                case 2:
                    //player.AttemptMove<Wall>(1, 0);
                    gameManager.playerMoving = true;
                    player.AttemptMove(1, 0);
                    break;
                case 3:
                    //player.AttemptMove<Wall>(0, -1);
                    gameManager.playerMoving = true;
                    player.AttemptMove(0, -1);
                    break;
                case 4:
                    //player.AttemptMove<Wall>(0, 1);
                    gameManager.playerMoving = true;
                    player.AttemptMove(0, 1);
                    break;
                default:
                    break;
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            //GameManager.instance.HandleHeuristicMode();
            gameManager.HandleHeuristicMode();
            GetComponent<DecisionRequester>().DecisionPeriod = 1;

            ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;

            //If it's not the player's turn, exit the function.
            //if (!CanMove())
            //{
            //    actionsOut[0] = lastAction;
            //    return;
            //}
            if (gameManager.playerMovesSinceEnemyMove == gameManager.playerMovesPerEnemyMove && CanMove() && gameManager.playerMoving == false)
            {
                discreteActionsOut[0] = lastAction;
                return;
            }

            gameManager.playerTurn = false;

            int horizontal = 0;     //Used to store the horizontal move direction.
            int vertical = 0;       //Used to store the vertical move direction.

            ////Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            //horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            ////Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            //vertical = (int)(Input.GetAxisRaw("Vertical"));

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
                horizontal = (int)(Input.GetAxisRaw("Horizontal"));
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
                vertical = (int)(Input.GetAxisRaw("Vertical"));
            }

            //Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
            {
                vertical = 0;
            }

            if (horizontal == 0 && vertical == 0)
            {
                discreteActionsOut[0] = 0;
            }
            else if (horizontal < 0)
            {
                discreteActionsOut[0] = 1;
            }
            else if (horizontal > 0)
            {
                discreteActionsOut[0] = 2;
            }
            else if (vertical < 0)
            {
                discreteActionsOut[0] = 3;
            }
            else if (vertical > 0)
            {
                discreteActionsOut[0] = 4;
            }

            discreteActionsOut[0] = discreteActionsOut[0] - 1; // TODO: Remove this line if zero movement is allowed
        }

        //public void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawSphere(new Vector3(playerNode.row + gameManager.trainingPenXPositionOffSet, playerNode.column + gameManager.trainingPenYPositionOffSet, 0), 0.25f);

        //    Gizmos.color = Color.green;
        //    Gizmos.DrawSphere(new Vector3(exitNode.row + gameManager.trainingPenXPositionOffSet, exitNode.column + gameManager.trainingPenYPositionOffSet, 0), 0.25f);
        //}
    }
}
