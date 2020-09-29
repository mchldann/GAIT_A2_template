using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public class TestResults : MonoBehaviour
    {
        public int Total, Level1, Level2, Level3, Level4, Level5, Level6, Level7, Level8, Level9, Level10, Level11, Level12, Level13, Level14, Level15;

        public void AddCount(int level)
        {
            Total++;
            switch (level)
            {
                case 1:
                    Level1++;
                    break;
                case 2:
                    Level2++;
                    break;
                case 3:
                    Level3++;
                    break;
                case 4:
                    Level4++;
                    break;
                case 5:
                    Level5++;
                    break;
                case 6:
                    Level6++;
                    break;
                case 7:
                    Level7++;
                    break;
                case 8:
                    Level8++;
                    break;
                case 9:
                    Level9++;
                    break;
                case 10:
                    Level10++;
                    break;
                case 11:
                    Level11++;
                    break;
                case 12:
                    Level12++;
                    break;
                case 13:
                    Level13++;
                    break;
                case 14:
                    Level14++;
                    break;
                case 15:
                    Level15++;
                    break;
                default:
                    Debug.Log(level);
                    break;
            }
        }
    }

}