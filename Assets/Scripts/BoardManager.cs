using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Completed
{
    public class BoardManager : MonoBehaviour
    {
        [Serializable]
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.

            //Assignment constructor.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }

        public int columns = 10;                                         //Number of columns in our game board.
        public int rows = 10;                                            //Number of rows in our game board.
        public Count wallCount = new Count(5, 9);                       //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count(1, 5);                       //Lower and upper limit for our random number of food items per level.
        public GameObject exit;                                         //Prefab to spawn for exit.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
                                                                        //public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public Enemy[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.

        private GameObject dynamicObjectsHolder;
        private GameObject boardHolder;                                 //A variable to store a reference to the transform of our Board object.
        private List<Vector3> gridPositions = new List<Vector3>();   //A list of possible locations to place tiles.

        [SerializeField] private Player player;
        [SerializeField] private GameManager gameManager;

        //Clears our list gridPositions and prepares it to generate a new board.
        void InitialiseList()
        {
            //Clear our list gridPositions.
            gridPositions.Clear();


            //Loop through x axis (columns).
            for (int x = 2; x < columns - 2; x++)
            {
                //Within each column, loop through y axis (rows).
                for (int y = 2; y < rows - 2; y++)
                {
                    //At each index add a new Vector3 to our list with the x and y coordinates of that position.
                    gridPositions.Add(new Vector3(x + gameManager.trainingPenXPositionOffSet, y + gameManager.trainingPenYPositionOffSet, 0f));
                    gameManager.gridState.nodes[x, y].objectOnTile = ObjectOnTile.none;
                    gameManager.gridState.nodes[x, y].unitOnTile = UnitOnTile.none;
                }
            }
        }

        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup()
        {
            //Loop along x axis, starting from -1 (to fill corner) with floor or outerwall edge tiles.
            for (int x = 0; x < columns; x++)
            {
                //Loop along y axis, starting from -1 to place floor or outerwall tiles.
                for (int y = 0; y < rows; y++)
                {
                    //Choose a random tile from our array of floor tile prefabs and prepare to instantiate it.
                    GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                    //Check if we current position is at board edge, if so choose a random outer wall prefab from our array of outer wall tiles.
                    if (x == 0 || x == columns - 1 || y == 0 || y == rows - 1)
                    {
                        toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        gameManager.gridState.nodes[x, y].objectOnTile = ObjectOnTile.outerWall;
                        gameManager.gridState.nodes[x, y].BlockingLayerObject = toInstantiate;
                    }

                    //Instantiate the GameObject instance using the prefab chosen for toInstantiate at the Vector3 corresponding to current grid position in loop, cast it to GameObject.
                    Instantiate(toInstantiate, new Vector3(x + gameManager.trainingPenXPositionOffSet, y + gameManager.trainingPenYPositionOffSet, 0f), Quaternion.identity, boardHolder.transform);
                }
            }
        }

        //RandomPosition returns a random position from our list gridPositions.
        Vector3 RandomPosition()
        {
            //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
            int randomIndex = Random.Range(0, gridPositions.Count);

            //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
            Vector3 randomPosition = gridPositions[randomIndex];

            //Remove the entry at randomIndex from the list so that it can't be re-used.
            gridPositions.RemoveAt(randomIndex);

            //Return the randomly selected Vector3 position.
            return randomPosition;
        }

        //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();

                //Choose a random tile from tileArray and assign it to tileChoice
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                GameObject copyobject = Instantiate(tileChoice, randomPosition, Quaternion.identity, dynamicObjectsHolder.transform);

                placeTilesOnGridState(copyobject, randomPosition);
            }
        }

        private void ClearOldScene()
        {
            if (boardHolder is null)
            {
                //Instantiate Board and set boardHolder to its transform.
                boardHolder = new GameObject("Board");

                //Instantiate Board and set boardHolder to its transform.
                dynamicObjectsHolder = new GameObject("DynamicObjects");
            }

            foreach (Transform child in dynamicObjectsHolder.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in boardHolder.transform)
            {
                Destroy(child.gameObject);
            }
            Array.Clear(gameManager.gridState.nodes, 0, gameManager.gridState.nodes.Length);
            gameManager.gridState.InitNodes();
        }

        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level)
        {
            ClearOldScene();

            //Creates the outer walls and floor.
            BoardSetup();

            //Reset our list of gridpositions.
            InitialiseList();

            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);
            //LayoutInnerWallsAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = (int)Mathf.Log(level, 2f);

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            //LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
            LayoutEnemiesAtRandom(enemyTiles, enemyCount, enemyCount);
            RandomisePlayerAndExitPositions();

        }

        public void RandomisePlayerAndExitPositions()
        {
            //GameObject parentLoader = transform.parent.gameObject;
            //GameObject parentofParentLoader = parentLoader.transform.parent.gameObject;
            //GameObject player_go = parentofParentLoader.transform.GetChild(1).gameObject;

            int playerPosition = Random.Range(0, 4);
            int exitPosition = (playerPosition + Random.Range(1, 4)) % 4;

            Vector3 newPlayerPos;
            switch (playerPosition)
            {
                case 0:
                    newPlayerPos = new Vector3(1f + gameManager.trainingPenXPositionOffSet, 1f + gameManager.trainingPenYPositionOffSet, 0f);

                    break;
                case 1:
                    newPlayerPos = new Vector3(1f + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                case 2:
                    newPlayerPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, 1f + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                case 3:
                    newPlayerPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                default:
                    newPlayerPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
            }

            //player_go.transform.position = newPlayerPos;
            //player_go.GetComponent<Player>().gridState = gridState;
            //player_go.GetComponent<Player>().gameManager = GetComponent<GameManager>();
            //player_go.GetComponent<Player>().smoothMovementEnd = newPlayerPos;

            player.transform.position = newPlayerPos;
            player.smoothMovementEnd = newPlayerPos;

            Vector3 exitPos;
            switch (exitPosition)
            {
                case 0:
                    exitPos = new Vector3(1f + gameManager.trainingPenXPositionOffSet, 1f + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                case 1:
                    exitPos = new Vector3(1f + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                case 2:
                    exitPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, 1f + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                case 3:
                    exitPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
                default:
                    exitPos = new Vector3((columns - 2) + gameManager.trainingPenXPositionOffSet, (rows - 2) + gameManager.trainingPenYPositionOffSet, 0f);
                    break;
            }

            //Instantiate the exit tile in the upper right hand corner of our game board
            Instantiate(exit, exitPos, Quaternion.identity, dynamicObjectsHolder.transform);

            // place player on gridState
            gameManager.gridState.nodes[(int)newPlayerPos.x - gameManager.trainingPenXPositionOffSet, (int)newPlayerPos.y - gameManager.trainingPenYPositionOffSet].unitOnTile = UnitOnTile.player;
            //gameManager.gridState.nodes[(int)newPlayerPos.x, (int)newPlayerPos.y].BlockingLayerObject = player;

            // place exit on gridsate
            gameManager.gridState.nodes[(int)exitPos.x - gameManager.trainingPenXPositionOffSet, (int)exitPos.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.exit;
        }

        public void placeTilesOnGridState(GameObject tileChoice, Vector3 randomPosition)
        {
            if (tileChoice.tag.Equals("BreakableWall"))
            {
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.innerWall;
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].BlockingLayerObject = tileChoice;
                //gameManager.gridState.nodes[(int)randomPosition.x, (int)randomPosition.y].innerWall = tileChoice;
            }
            if (tileChoice.tag.Equals("Food"))
            {
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.food;
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].foodObject = tileChoice;
            }
            if (tileChoice.tag.Equals("Soda"))
            {
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.soda;
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].foodObject = tileChoice;
            }
            //if (tileChoice.tag.Equals("Enemy"))
            //{
            //	gridState.nodes[(int)randomPosition.x, (int)randomPosition.y].unitOnTile = UnitOnTile.enemy;
            //	gridState.nodes[(int)randomPosition.x, (int)randomPosition.y].BlockingLayerObject = tileChoice;
            //}
        }

        public void LayoutEnemiesAtRandom(Enemy[] tileArray, int minimum, int maximum)
        {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();

                //Choose a random tile from tileArray and assign it to tileChoice
                Enemy tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                Enemy copyEnemy = Instantiate(tileChoice, randomPosition, Quaternion.identity, dynamicObjectsHolder.transform);

                placeEnemiesOnGridState(copyEnemy, randomPosition);
            }
        }

        public void LayoutInnerWallsAtRandom(Wall[] tileArray, int minimum, int maximum)
        {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();

                //Choose a random tile from tileArray and assign it to tileChoice
                Wall tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation

                Instantiate(tileChoice, new Vector3(randomPosition.x + gameManager.trainingPenXPositionOffSet, randomPosition.y + gameManager.trainingPenYPositionOffSet, 0), Quaternion.identity, dynamicObjectsHolder.transform);

                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.innerWall;
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].BlockingLayerObject = tileChoice.gameObject;
                gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].innerWall = tileChoice;
            }
        }
        public void placeEnemiesOnGridState(Enemy tileChoice, Vector3 randomPosition)
        {
            gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].unitOnTile = UnitOnTile.enemy;
            gameManager.gridState.nodes[(int)randomPosition.x - gameManager.trainingPenXPositionOffSet, (int)randomPosition.y - gameManager.trainingPenYPositionOffSet].BlockingLayerObject = tileChoice.gameObject;
            gameManager.AddEnemyToList(tileChoice);
        }
    }
}
