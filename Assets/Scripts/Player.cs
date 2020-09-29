using UnityEngine;
using UnityEngine.UI;	//Allows us to use UI.

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{

		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.
		
		private Animator animator;					//Used to store a reference to the Player's animator component.
        private PlayerAgent agent;                  //Used to store a reference to the Player's agent component.

        public bool levelFinished = false;
        public bool gameOver = false;

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        private Vector2 touchOrigin = -Vector2.one;	//Used to store location of screen touch origin for mobile controls.
#endif


        //Start overrides the Start function of MovingObject
        protected override void Start ()
		{
            //Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

            //Get a component reference to the Player's agent component
            agent = GetComponent<PlayerAgent>();
			
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		

		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		public override void AttemptMove <T> (int xDir, int yDir)
		{
			
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			

            GameManager.instance.playerMovesSinceSheepMove++;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");
		}
		

		//Restart reloads the scene when called.
		private void Restart ()
		{
            agent.HandleLevelRestart(gameOver);

            if (gameOver)
            {
                GameManager.instance.level = 0;
            }

            //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
            //and not load all the scene object in the current scene.
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);

            GameManager.instance.CreateNewLevel();

            levelFinished = false;
            gameOver = false;
            enabled = true;
        }
		
		
        //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
        private void CheckIfGameOver()
        {
            if (levelFinished || gameOver)
            {
                return;
            }

			/*
            //Check if food point total is less than or equal to zero.
            if (food <= 0)
            {
                //Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
                SoundManager.instance.PlaySingle(gameOverSound);

                //Disable the player object since level is over.
                enabled = false;

                //Stop the background music.
                //SoundManager.instance.musicSource.Stop();

                //Call the GameOver function of GameManager.
                GameManager.instance.GameOver();

                gameOver = true;

                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", GameManager.instance.restartLevelDelay);
            }
			*/
        }
    }
}

