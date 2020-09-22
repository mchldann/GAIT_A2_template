using UnityEngine;
using System.Collections;

namespace Completed
{
	//Sheep inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Sheep : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.
		
		
		private Animator animator;							//Variable of type Animator to store a reference to the sheep's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.
		private bool skipMove;								//Boolean to determine whether or not sheep should skip a turn or move this turn.
		
		
		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			//Register this sheep with our instance of GameManager by adding it to a list of Sheep objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddSheepToList (this);
			
			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();
			
			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;
			
			//Call the start function of our base class MovingObject.
			base.Start ();
		}
		
		
		//Override the AttemptMove function of MovingObject to include functionality needed for Sheep to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		public override void AttemptMove <T> (int xDir, int yDir)
		{
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove)
			{
				skipMove = false;
				return;
				
			}
			
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);
			
			//Now that Sheep has moved, set skipMove to true to skip next move.
			skipMove = true;
		}
		
		
		//MoveSheep is called by the GameManger each turn to tell each Sheep to try to away from the player.
		public void MoveSheep ()
		{
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;
			
			float xDiff = target.position.x - transform.position.x;
			float yDiff = target.position.y - transform.position.y;

			if (Mathf.Abs(xDiff) < Mathf.Abs(yDiff))
				xDir = xDiff > 0.0f ? -1 : 1;
			else
				yDir = yDiff > 0.0f ? -1 : 1;
			
			//Call the AttemptMove function and pass in the generic parameter Player, because Sheep is moving and expecting to potentially encounter a Player
			AttemptMove <Player> (xDir, yDir);
		}
		
		
		//OnCantMove is called if Sheep attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;
			
			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);
			
			//Set the attack trigger of animator to trigger Sheep attack animation.
			animator.SetTrigger ("sheepAttack");
			
			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
