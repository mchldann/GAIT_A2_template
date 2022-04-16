using UnityEngine;
using System.Collections;

namespace Completed
{
    //Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
    public class Enemy : MonoBehaviour
    {
        public int playerDamage;                            //The amount of food points to subtract from the player when attacking.
                                                            //public AudioClip attackSound1;                      //First of two audio clips to play when attacking the player.
                                                            //public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.


        private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
        //private Transform target;                           //Transform to attempt to move toward each turn.
        private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.
        public Vector3 smoothMovementEnd;
        public bool isMoving = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        public void AttemptMove(int xDir, int yDir, GameManager gameManager)
        {
            //Check if skipMove is true, if so set it to false and skip this turn.
            if (skipMove)
            {
                skipMove = false;
                return;

            }

            Vector2 attemptingPosition = new Vector2(transform.position.x + xDir, transform.position.y + yDir);

            //Call the AttemptMove function from MovingObject.
            //base.AttemptMove(xDir, yDir);

            //Set canMove to true if Move was successful, false if failed.
            //bool canMove = Move(xDir, yDir, out hit);
            bool canMove = Move(xDir, yDir, gameManager);

            if (!gameManager.gridState.CheckIfNodeHasNoCollisions((int)attemptingPosition.x - gameManager.trainingPenXPositionOffSet, (int)attemptingPosition.y - gameManager.trainingPenYPositionOffSet))
            {
                //If nothing was hit, return and don't execute further code.
                return;
            }

            if (!canMove && gameManager.player.transform.position.Equals(new Vector2(attemptingPosition.x, attemptingPosition.y)))
            {
                gameManager.player.LoseFood(playerDamage);
                animator.SetTrigger("enemyAttack");
            }

            //Now that Enemy has moved, set skipMove to true to skip next move.
            skipMove = true;
        }


        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy(GameManager gameManager)
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            //if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            if (Mathf.Abs(gameManager.player.transform.position.x - transform.position.x) < float.Epsilon)

                //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                yDir = gameManager.player.transform.position.y > transform.position.y ? 1 : -1;

            //If the difference in positions is not approximately zero (Epsilon) do the following:
            else
                //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                xDir = gameManager.player.transform.position.x > transform.position.x ? 1 : -1;

            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove(xDir, yDir, gameManager);
        }

        //Move returns true if it is able to move and false if not. 
        //Move takes parameters for x direction, y direction and a RaycastHit2D to check collision.
        //protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
        protected bool Move(int xDir, int yDir, GameManager gameManager)
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

            //Check if anything was hit
            //if (hit.transform == null)
            if (!gameManager.gridState.CheckIfNodeHasNoCollisions((int)end.x - gameManager.trainingPenXPositionOffSet, (int)end.y - gameManager.trainingPenYPositionOffSet))
            {
                //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
                StartCoroutine(SmoothMovement(end, gameManager));
                gameManager.gridState.moveUnitOnGridState((int)start.x - gameManager.trainingPenXPositionOffSet, (int)start.y - gameManager.trainingPenYPositionOffSet, (int)end.x - gameManager.trainingPenXPositionOffSet, (int)end.y - gameManager.trainingPenYPositionOffSet);
                //Return true to say that Move was successful
                return true;
            }
            else
            {
                StartCoroutine(DoNotMove(gameManager));
            }

            //If something was hit, return false, Move was unsuccesful.
            return false;
        }

        protected IEnumerator DoNotMove(GameManager gameManager)
        {
            isMoving = true;

            yield return new WaitForSeconds(gameManager.moveTime);

            isMoving = false;
        }

        //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
        protected IEnumerator SmoothMovement(Vector3 end, GameManager gameManager)
        {
            isMoving = true;

            smoothMovementEnd = end;

            while (true)
            {
                //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
                //Square magnitude is used instead of magnitude because it's computationally cheaper.
                float sqrRemainingDistance = (transform.position - smoothMovementEnd).sqrMagnitude;

                if (sqrRemainingDistance <= float.Epsilon)
                {
                    break;
                }

                //Find a new position proportionally closer to the end, based on the moveTime
                //Vector3 newPostion = Vector3.MoveTowards(rb2D.position, smoothMovementEnd, GameManager.instance.inverseMoveTime * Time.deltaTime);

                //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
                //rb2D.MovePosition(newPostion);
                //transform.position = newPosition;
                transform.position = smoothMovementEnd;

                //Return and loop until sqrRemainingDistance is close enough to zero to end the function
                yield return null;
            }

            isMoving = false;
        }

        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        //protected void OnCantMove<T>(T component)
        //{
        //    //Declare hitPlayer and set it to equal the encountered component.
        //    Player hitPlayer = component as Player;

        //    //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
        //    hitPlayer.LoseFood(playerDamage);

        //    //Set the attack trigger of animator to trigger Enemy attack animation.
        //    //animator.SetTrigger("enemyAttack");

        //    //Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
        //    //SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        //}
    }
}
