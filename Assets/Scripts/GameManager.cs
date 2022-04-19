using UnityEngine;
using System.Collections;

namespace Completed
{
    using System.Collections.Generic;       //Allows us to use Lists. 
    using UnityEngine.UI;                   //Allows us to use UI.

    public class GameManager : MonoBehaviour
    {
        public bool trainingMode = false;
        public float moveTime = 20f;               //Time it will take object to move, in seconds.
                                                   //public float inverseMoveTime;               //Used to make movement more efficient.
        public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.

        //public const int playerStartFood = 100;
        public int playerStartFood = 100;

        public float levelStartDelay = 2f;                      //Time to wait before starting level, in seconds.
        public float turnDelay = 0.1f;                          //Delay between each Player turn.
                                                                //public static GameManager instance = null;              //Static instance of GameManager which allows it to be accessed by any other script.

        //public const int playerMovesPerEnemyMove = 2;
        public int playerMovesPerEnemyMove = 2;
        public int playerMovesSinceEnemyMove = 0;

        public Text levelText;                                 //Text to display current level number.
                                                               //private GameObject levelImage;                          //Image to block out level as levels are being set up, background for levelText.
        private BoardManager boardScript;                       //Store a reference to our BoardManager which will set up the level.
        public int level = 3;                                   //Current level number, expressed in game as "Day 1".
        private List<Enemy> enemies;                            //List of all Enemy units, used to issue them move commands.
        private bool enemiesMoving;                             //Boolean to check if enemies are moving.
        public bool doingSetup = true;                          //Boolean to check if we're setting up board, prevent Player from moving during setup.

        [Tooltip("Offsets cannot be negative integers")]
        public int trainingPenXPositionOffSet;
        [Tooltip("Offsets cannot be negative integers")]
        public int trainingPenYPositionOffSet;
        public GridState gridState;
        [SerializeField] public Player player;

        public bool playerTurn = true;
        public bool playerMoving;

        public void CreateNewLevel()
        {
            //instance.level++;
            //instance.InitGame();
            level++;
            InitGame();
        }

        //Awake is always called before any Start functions
        void Awake()
        {
            gridState = new GridState();
            // These values will be updated if the game is started in heuristic mode.
            //Time.fixedDeltaTime = 0.1f;
            //levelStartDelay = 0.001f;
            //restartLevelDelay = 0.001f;
            //turnDelay = 0.001f;
            //moveTime = 0.001f;

            //By storing the reciprocal of the move time we can use it by multiplying instead of dividing, this is more efficient.
            //inverseMoveTime = 1f / moveTime;

            ////Check if instance already exists
            //if (instance == null)

            //	//if not, set instance to this
            //	instance = this;

            ////If instance already exists and it's not this:
            //else if (instance != this)

            //	//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            //	Destroy(gameObject);

            ////Sets this to not be destroyed when reloading scene
            //DontDestroyOnLoad(gameObject);

            //Assign enemies to a new List of Enemy objects.
            enemies = new List<Enemy>();

            //Get a component reference to the attached BoardManager script
            boardScript = GetComponent<BoardManager>();

            //boardScript.trainingPenXPositionOffSet = trainingPenXPositionOffSet;
            //boardScript.trainingPenYPositionOffSet = trainingPenYPositionOffSet;
            //boardScript.gridState = gridState;

            //Call the InitGame function to initialize the first level 
            InitGame();
        }
        
        //Initializes the game for each level.
        void InitGame()
        {
            //While doingSetup is true the player can't move, prevent player from moving while title card is up.
            doingSetup = true;

            //Get a reference to our image LevelImage by finding it by name.
            //if (levelImage is null)
            //{
            //    levelImage = GameObject.Find("LevelImage");

            //    //Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
            //    levelText = GameObject.Find("LevelText").GetComponent<Text>();
            //}

            //Set the text of levelText to the string "Day" and append the current level number.
            levelText.text = "Day " + level;

            //Set levelImage to active blocking player's view of the game board during setup.
            //levelImage.SetActive(true);

            //Call the HideLevelImage function with a delay in seconds of levelStartDelay.
            Invoke("HideLevelImage", levelStartDelay);

            //Clear any Enemy objects in our List to prepare for next level.
            enemies.Clear();

            //Call the SetupScene function of the BoardManager script, pass it current level number.
            boardScript.SetupScene(level);

            playerMovesSinceEnemyMove = 0;
            doingSetup = false;
            //playerTurn = true;
        }


        //Hides black image used between levels
        void HideLevelImage()
        {
            //Disable the levelImage gameObject.
            //levelImage.SetActive(false);

            //Set doingSetup to false allowing player to move again.
            doingSetup = false;
            playerMovesSinceEnemyMove = 0;
        }

        //Update is called every frame.
        void Update()
        {
            //Check that playersTurn or enemiesMoving or doingSetup are not currently true.
            if ((playerMovesSinceEnemyMove < playerMovesPerEnemyMove) || enemiesMoving || doingSetup)

                //If any of these are true, return and do not start MoveEnemies.
                return;

            //Start moving enemies.
            playerMovesSinceEnemyMove = 0;
            StartCoroutine(MoveEnemies());
        }

        //Call this to add the passed in Enemy to the List of Enemy objects.
        public void AddEnemyToList(Enemy script)
        {
            //Add Enemy to List enemies.
            enemies.Add(script);
        }

        //GameOver is called when the player reaches 0 food points
        public void GameOver()
        {
            //Set levelText to display number of levels passed and game over message
            levelText.text = "After " + level + " days, you starved.";

            //Enable black background image gameObject.
            //levelImage.SetActive(true);

            //Disable this GameManager.
            //enabled = false;
        }

        //Coroutine to move enemies in sequence.
        IEnumerator MoveEnemies()
        {
            enemiesMoving = true;

            //Wait for turnDelay seconds, defaults to .1 (100 ms).
            yield return new WaitForSeconds(turnDelay);

            //If there are no enemies spawned (IE in first level):
            if (enemies.Count == 0)
            {
                //Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
                yield return new WaitForSeconds(turnDelay);
            }

            //Loop through List of Enemy objects.
            for (int i = 0; i < enemies.Count; i++)
            {
                //Call the MoveEnemy function of Enemy at index i in the enemies List.
                enemies[i].MoveEnemy(this);

                //Wait for Enemy's moveTime before moving next Enemy, 
                //yield return new WaitForSeconds(GameManager.instance.moveTime);
                yield return new WaitForSeconds(moveTime);
            }

            //Enemies are done moving, set enemiesMoving to false.
            enemiesMoving = false;
        }
    }
}
