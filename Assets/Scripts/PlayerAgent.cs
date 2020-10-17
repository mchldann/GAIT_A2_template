using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

namespace Completed
{
    //Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
    public class PlayerAgent : Agent
    {
        private Player player;
        private int lastAction = 0;

        void Start()
        {
            player = GetComponent<Player>();
        }

        public override void OnEpisodeBegin()
        {
            // TODO: Add any necessary code
            GameManager.instance.CreateNewLevel();
        }

        public void HandleRestartTest()
        {
            AddReward(-1.0f);
        }

        public void HandleSheepScore()
        {
            AddReward(1.0f);
        }

        public void HandleAttemptMove()
        {
            // TODO: Change the reward below as appropriate. If you want to add a cost per move, you could change the reward to -1.0f (for example).
            AddReward(-0.01f);
        }

        public void HandleFinishlevel()
        {
            // TODO: Change the reward below as appropriate.
            float reward = 5 / (player.moves + 1f);
            AddReward(reward);
        }

        public void HandleLevelRestart(bool gameOver)
        {
            if (gameOver)
            {
                Academy.Instance.StatsRecorder.Add("Level Reached", GameManager.instance.level);
                EndEpisode();
            }   
            else
            {
                // Probably *is* best to consider episodes finished when the exit is reached
                EndEpisode();
            }
            
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // TODO: Insert proper code here for collecting the observations!
            // At the moment this code just feeds in 10 observations, all hardcoded to zero, as a placeholder.
            int totalObservers = 27;

            Vector3 pos = player.transform.position;
            sensor.AddObservation(GameManager.instance.exit.transform.position - pos);
            int count = 6;
            foreach (Sheep sheep in GameManager.instance.sheep)
            {
                sensor.AddObservation(sheep.transform.position - pos);
                count += 3;
            }

            for (int i = 0; i < totalObservers - count; i++)
            {
                sensor.AddObservation(0.0f);
            }

            base.CollectObservations(sensor);
        }

        private bool CanMove()
        {
            return !(player.isMoving || player.levelFinished || player.gameOver || GameManager.instance.doingSetup);
        }

        public override void OnActionReceived(float[] vectorAction)
        {
            //If it's not the player's turn, exit the function.
            if (!CanMove()) return;

            lastAction = (int)vectorAction[0] + 1; // To allow standing still as an action, remove the +1 and change "Branch 0 size" to 5.

            switch (lastAction)
            {
                case 0:
                    break;
                case 1:
                    player.AttemptMove<Wall>(-1, 0);
                    break;
                case 2:
                    player.AttemptMove<Wall>(1, 0);
                    break;
                case 3:
                    player.AttemptMove<Wall>(0, -1);
                    break;
                case 4:
                    player.AttemptMove<Wall>(0, 1);
                    break;
                default:
                    break;
            }
        }

        public override void Heuristic(float[] actionsOut)
        {
            GameManager.instance.HandleHeuristicMode();
            GetComponent<DecisionRequester>().DecisionPeriod = 1;

            //If it's not the player's turn, exit the function.
            if (!CanMove())
            {
                actionsOut[0] = lastAction;
                return;
            }

            int horizontal = 0;     //Used to store the horizontal move direction.
            int vertical = 0;       //Used to store the vertical move direction.

            //Check if we are running either in the Unity editor or in a standalone build.
#if UNITY_STANDALONE || UNITY_WEBPLAYER

            //Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            horizontal = (int)(Input.GetAxisRaw("Horizontal"));

            //Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            vertical = (int)(Input.GetAxisRaw("Vertical"));

            //Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
            {
                vertical = 0;
            }
            //Check if we are running on iOS, Android, Windows Phone 8 or Unity iPhone
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
			
			//Check if Input has registered more than zero touches
			if (Input.touchCount > 0)
			{
				//Store the first touch detected.
				Touch myTouch = Input.touches[0];
				
				//Check if the phase of that touch equals Began
				if (myTouch.phase == TouchPhase.Began)
				{
					//If so, set touchOrigin to the position of that touch
					touchOrigin = myTouch.position;
				}
				
				//If the touch phase is not Began, and instead is equal to Ended and the x of touchOrigin is greater or equal to zero:
				else if (myTouch.phase == TouchPhase.Ended && touchOrigin.x >= 0)
				{
					//Set touchEnd to equal the position of this touch
					Vector2 touchEnd = myTouch.position;
					
					//Calculate the difference between the beginning and end of the touch on the x axis.
					float x = touchEnd.x - touchOrigin.x;
					
					//Calculate the difference between the beginning and end of the touch on the y axis.
					float y = touchEnd.y - touchOrigin.y;
					
					//Set touchOrigin.x to -1 so that our else if statement will evaluate false and not repeat immediately.
					touchOrigin.x = -1;
					
					//Check if the difference along the x axis is greater than the difference along the y axis.
					if (Mathf.Abs(x) > Mathf.Abs(y))
						//If x is greater than zero, set horizontal to 1, otherwise set it to -1
						horizontal = x > 0 ? 1 : -1;
					else
						//If y is greater than zero, set horizontal to 1, otherwise set it to -1
						vertical = y > 0 ? 1 : -1;
				}
			}
			
#endif //End of mobile platform dependendent compilation section started above with #elif

            if (horizontal == 0 && vertical == 0)
            {
                actionsOut[0] = 0;
            }
            else if (horizontal < 0)
            {
                actionsOut[0] = 1;
            }
            else if (horizontal > 0)
            {
                actionsOut[0] = 2;
            }
            else if (vertical < 0)
            {
                actionsOut[0] = 3;
            }
            else if (vertical > 0)
            {
                actionsOut[0] = 4;
            }

            actionsOut[0] = actionsOut[0] - 1; // TODO: Remove this line if zero movement is allowed
        }
    }
}
