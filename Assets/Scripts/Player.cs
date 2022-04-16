using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;   //Allows us to use UI.

namespace Completed
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class Player : MonoBehaviour
    {
        //public GameManager gameManager;
        public int pointsPerFood = 10;              //Number of points to add to player food points when picking up a food object.
        public int pointsPerSoda = 20;              //Number of points to add to player food points when picking up a soda object.
        public int wallDamage = 1;                  //How much damage a player does to a wall when chopping it.
        public Text foodText;                       //UI Text to display current player food total.
                                                    //public AudioClip moveSound1;                //1 of 2 Audio clips to play when player moves.
                                                    //public AudioClip moveSound2;                //2 of 2 Audio clips to play when player moves.
                                                    //public AudioClip eatSound1;                 //1 of 2 Audio clips to play when player collects a food object.
                                                    //public AudioClip eatSound2;                 //2 of 2 Audio clips to play when player collects a food object.
                                                    //public AudioClip drinkSound1;               //1 of 2 Audio clips to play when player collects a soda object.
                                                    //public AudioClip drinkSound2;               //2 of 2 Audio clips to play when player collects a soda object.
                                                    //public AudioClip gameOverSound;             //Audio clip to play when player dies.

        private Animator animator;                  //Used to store a reference to the Player's animator component.
        public int food;                            //Used to store player food points total during level.

        private PlayerAgent agent;                  //Used to store a reference to the Player's agent component.

        public bool levelFinished = false;
        public bool gameOver = false;

        public Vector3 smoothMovementEnd;
        public bool isMoving = false;

        //#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        //        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
        //#endif

        [SerializeField] protected GameManager gameManager;

        //Start overrides the Start function of MovingObject
        //protected override void Start()
        private void Start()
        {
            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

            //Get a component reference to the Player's agent component
            agent = GetComponent<PlayerAgent>();

            //Get the current food point total stored in GameManager.instance between levels.
            //food = GameManager.playerStartFood;
            food = gameManager.playerStartFood;

            //Set the foodText to reflect the current player food total.
            foodText.text = "Food: " + food;

            //Call the Start function of the MovingObject base class.
            //base.Start();
        }


        //AttemptMove overrides the AttemptMove function in the base class MovingObject
        //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        public void AttemptMove(int xDir, int yDir)
        {
            //Every time player moves, subtract from food points total.
            food--;
            agent.HandleAttemptMove();

            //Update food text display to reflect current score.
            foodText.text = "Food: " + food;

            //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            //base.AttemptMove(xDir, yDir);

            Vector2 attemptingPosition = new Vector2(transform.position.x + xDir, transform.position.y + yDir);

            //Hit allows us to reference the result of the Linecast done in Move.
            //RaycastHit2D hit;

            //If Move returns true, meaning Player was able to move into an empty space.
            //if (Move(xDir, yDir, out hit))
            if (Move(xDir, yDir))
            {
                //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                //SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }
            else
            {
                if (gameManager.gridState.nodes[(int)attemptingPosition.x - gameManager.trainingPenXPositionOffSet, (int)attemptingPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile == ObjectOnTile.innerWall)
                {
                    Wall currentWall = gameManager.gridState.nodes[(int)attemptingPosition.x - gameManager.trainingPenXPositionOffSet, (int)attemptingPosition.y - gameManager.trainingPenYPositionOffSet].BlockingLayerObject.GetComponent<Wall>();
                    currentWall.DamageWall(wallDamage);
                    animator.SetTrigger("playerChop");
                    if (currentWall.hp <= 0)
                    {
                        gameManager.gridState.nodes[(int)attemptingPosition.x - gameManager.trainingPenXPositionOffSet, (int)attemptingPosition.y - gameManager.trainingPenYPositionOffSet].objectOnTile = ObjectOnTile.none;
                    }
                }
            }

            //Since the player has moved and lost food points, check if the game has ended.
            CheckIfGameOver();

            CheckIfMovedOntoObject();

            //GameManager.instance.playerMovesSinceEnemyMove++;
            gameManager.playerMovesSinceEnemyMove++;
        }

        //Move returns true if it is able to move and false if not. 
        //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
        //protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        protected bool Move(int xDir, int yDir)
        {
            //Store start position to move from, based on objects current transform position.
            Vector2 start = transform.position;

            // Calculate end position based on the direction parameters passed in when calling Move.
            Vector2 end = start + new Vector2(xDir, yDir);

            //Disable the boxCollider so that linecast doesn't hit this object's own collider.
            //boxCollider.enabled = false;

            //Cast a line from start point to end point checking collision on blockingLayer.
            //hit = Physics2D.Linecast(start, end, blockingLayer);


            //Re-enable boxCollider after linecast
            //boxCollider.enabled = true;

            //gameManager.gridState.CheckIfNodeHasNoCollisions((int)end.x, (int)end.y);

            //Check if anything was hit
            //if (hit.transform == null)
            if (!gameManager.gridState.CheckIfNodeHasNoCollisions((int)end.x - gameManager.trainingPenXPositionOffSet, (int)end.y - gameManager.trainingPenYPositionOffSet))
            {
                //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
                StartCoroutine(SmoothMovement(end));
                gameManager.gridState.moveUnitOnGridState((int)start.x - gameManager.trainingPenXPositionOffSet, (int)start.y - gameManager.trainingPenYPositionOffSet, (int)end.x - gameManager.trainingPenXPositionOffSet, (int)end.y - gameManager.trainingPenYPositionOffSet);
                //Return true to say that Move was successful
                return true;
            }
            else
            {
                StartCoroutine(DoNotMove());
            }

            //If something was hit, return false, Move was unsuccesful.
            return false;
        }

        private void CheckIfMovedOntoObject()
        {
            Node playersCurrentNode = gameManager.gridState.GetPlayerNode();

            if (levelFinished || gameOver)
            {
                return;
            }

            if (playersCurrentNode.objectOnTile == ObjectOnTile.exit)
            {
                agent.HandleFinishlevel();

                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", gameManager.restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;

                levelFinished = true;
            }
            //Check if the tag of the trigger collided with is Food.
            else if (playersCurrentNode.objectOnTile == ObjectOnTile.food)
            {
                //Add pointsPerFood to the players current food total.
                food += pointsPerFood;
                agent.HandleFoundFood();

                //Update foodText to represent current total and notify player that they gained points
                foodText.text = "+" + pointsPerFood + " Food: " + food;

                playersCurrentNode.objectOnTile = ObjectOnTile.none;

                //Disable the food object the player collided with.
                playersCurrentNode.foodObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is Soda.
            else if (playersCurrentNode.objectOnTile == ObjectOnTile.soda)
            {
                //Add pointsPerSoda to players food points total
                food += pointsPerSoda;
                agent.HandleFoundSoda();

                //Update foodText to represent current total and notify player that they gained points
                foodText.text = "+" + pointsPerSoda + " Food: " + food;

                //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
                //SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

                playersCurrentNode.objectOnTile = ObjectOnTile.none;

                //Disable the soda object the player collided with.
                playersCurrentNode.foodObject.SetActive(false);
            }
        }

        protected IEnumerator DoNotMove()
        {
            isMoving = true;

            yield return new WaitForSeconds(gameManager.moveTime);

            isMoving = false;
        }

        //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
        protected IEnumerator SmoothMovement(Vector3 end)
        {
            isMoving = true;

            smoothMovementEnd = end;

            while (true)
            {
                //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
                //Square magnitude is used instead of magnitude because it's computationally cheaper.
                //float sqrRemainingDistance = (transform.position - smoothMovementEnd).sqrMagnitude;

                //if (sqrRemainingDistance <= float.Epsilon)
                //{
                if (Vector3.Distance(transform.position, smoothMovementEnd) == 0f)
                {
                    //Debug.Log("fuck");
                    //gameManager.playerTurn = true;
                    //gameManager.playerMoving = true;
                    break;
                }

                //Find a new position proportionally closer to the end, based on the moveTime
                //Vector3 newPostion = Vector3.MoveTowards(rb2D.position, smoothMovementEnd, GameManager.instance.inverseMoveTime * Time.deltaTime);
                //Vector3 newPosition = Vector3.MoveTowards(transform.position, smoothMovementEnd, gameManager.inverseMoveTime * Time.deltaTime);

                //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
                //rb2D.MovePosition(newPostion);
                //transform.position = newPosition;
                transform.position = smoothMovementEnd;

                //Return and loop until sqrRemainingDistance is close enough to zero to end the function
                yield return null;
                //yield return new WaitForSeconds(gameManager.moveTime);
            }
            isMoving = false;
            //gameManager.playerMoving = false;
            //gameManager.playerTurn = true;
        }


        //OnCantMove overrides the abstract function OnCantMove in MovingObject.
        //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        //protected override void OnCantMove(T component)
        //{
        //    //Set hitWall to equal the component passed in as a parameter.
        //    Wall hitWall = component as Wall;

        //    //Call the DamageWall function of the Wall we are hitting.
        //    hitWall.DamageWall(wallDamage);

        //    //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
        //    //animator.SetTrigger("playerChop");
        //}


        //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        //private void OnTriggerEnter2D(Collider2D other)
        //{
        //    if (levelFinished || gameOver)
        //    {
        //        return;
        //    }

        //    //Check if the tag of the trigger collided with is Exit.
        //    if (other.tag == "Exit")
        //    {
        //        //agent.HandleFinishlevel();

        //        //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
        //        //Invoke("Restart", GameManager.instance.restartLevelDelay);
        //        Invoke("Restart", gameManager.restartLevelDelay);

        //        //Disable the player object since level is over.
        //        enabled = false;

        //        levelFinished = true;
        //    }

        //    //Check if the tag of the trigger collided with is Food.
        //    else if (other.tag == "Food")
        //    {
        //        //Add pointsPerFood to the players current food total.
        //        food += pointsPerFood;
        //        //agent.HandleFoundFood();

        //        //Update foodText to represent current total and notify player that they gained points
        //        foodText.text = "+" + pointsPerFood + " Food: " + food;

        //        //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
        //        //SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

        //        //Disable the food object the player collided with.
        //        other.gameObject.SetActive(false);
        //    }

        //    //Check if the tag of the trigger collided with is Soda.
        //    else if (other.tag == "Soda")
        //    {
        //        //Add pointsPerSoda to players food points total
        //        food += pointsPerSoda;
        //        //agent.HandleFoundSoda();

        //        //Update foodText to represent current total and notify player that they gained points
        //        foodText.text = "+" + pointsPerSoda + " Food: " + food;

        //        //Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
        //        //SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);

        //        //Disable the soda object the player collided with.
        //        other.gameObject.SetActive(false);
        //    }
        //}


        //Restart reloads the scene when called.
        private void Restart()
        {
            agent.HandleLevelRestart(gameOver);

            if (gameOver)
            {
                //GameManager.instance.level = 0;
                gameManager.level = 0;
                gameManager.restartLevelDelay = 0;
                //food = GameManager.playerStartFood;
                food = gameManager.playerStartFood;
                foodText.text = "Food: " + food;
            }

            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);

            //GameManager.instance.CreateNewLevel();
            gameManager.CreateNewLevel();

            levelFinished = false;
            gameOver = false;
            enabled = true;
        }


        //LoseFood is called when an enemy attacks the player.
        //It takes a parameter loss which specifies how many points to lose.
        public void LoseFood(int loss)
        {
            //Set the trigger for the player animator to transition to the playerHit animation.
            animator.SetTrigger("playerHit");

            //Subtract lost food points from the players total.
            food -= loss;
            agent.HandleLoseFood(loss);

            //Update the food display with the new total.
            foodText.text = "-" + loss + " Food: " + food;

            //Check to see if game has ended.
            CheckIfGameOver();
        }


        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            if (levelFinished || gameOver)
            {
                return;
            }

            //Check if food point total is less than or equal to zero.
            if (food <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                //SoundManager.instance.PlaySingle(gameOverSound);

                //Disable the player object since level is over.
                enabled = false;

                //Stop the background music.
                //SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                //GameManager.instance.GameOver();
                gameManager.GameOver();

                gameOver = true;

                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                //Invoke("Restart", GameManager.instance.restartLevelDelay);
                Invoke("Restart", gameManager.restartLevelDelay);
            }
        }
    }
}

