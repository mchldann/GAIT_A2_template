using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
    public class GridState
    {
        private static int columns = 10;
        private static int rows = 10;
        public Node[,] nodes = new Node[rows, columns];
        public Node playerNode;

        public void InitNodes()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    nodes[i, j] = new Node(i, j);
                }
            }
        }

        public bool CheckIfNodeHasNoCollisions(int requestedX, int requestedY)
        {
            Node checkingNode = nodes[requestedX, requestedY];
            if (checkingNode.objectOnTile == ObjectOnTile.outerWall)
            {
                return true;
            }
            if (checkingNode.objectOnTile == ObjectOnTile.innerWall)
            {
                return true;
            }
            if (checkingNode.unitOnTile == UnitOnTile.player)
            {
                return true;
            }
            if (checkingNode.unitOnTile == UnitOnTile.enemy)
            {
                return true;
            }
            return false;
        }

        public void moveUnitOnGridState(int currentNodeX, int currentNodeY, int endNodeX, int endNodeY)
        {
            UnitOnTile tempUnit = nodes[currentNodeX, currentNodeY].unitOnTile;
            nodes[currentNodeX, currentNodeY].unitOnTile = UnitOnTile.none;
            nodes[endNodeX, endNodeY].unitOnTile = tempUnit;

            GameObject tempGameObject = nodes[currentNodeX, currentNodeY].BlockingLayerObject;
            // since we are just clearing the array and not destroying the object here, I assume this should be fine(?)
            nodes[currentNodeX, currentNodeY].BlockingLayerObject = null;
            nodes[endNodeX, endNodeY].BlockingLayerObject = tempGameObject;
        }

        public Node GetPlayerNode()
        {
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].unitOnTile == UnitOnTile.player)
                    {
                        return nodes[x, y];
                    }
                }
            }
            return null;
        }

        public Node GetExitNode()
        {
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].objectOnTile == ObjectOnTile.exit)
                    {
                        return nodes[x, y];
                    }
                }
            }
            return null;
        }

        public List<Node> GetListOfNodesWithFood()
        {
            List<Node> returnList = new List<Node>();
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].objectOnTile == ObjectOnTile.food)
                    {
                        returnList.Add(nodes[x, y]);
                    }
                }
            }
            return returnList;
        }

        public List<Node> GetListOfNodesWithSoda()
        {
            List<Node> returnList = new List<Node>();
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].objectOnTile == ObjectOnTile.soda)
                    {
                        returnList.Add(nodes[x, y]);
                    }
                }
            }
            return returnList;
        }

        public List<Node> GetListOfNodesWithInnerWalls()
        {
            List<Node> returnList = new List<Node>();
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].objectOnTile == ObjectOnTile.innerWall)
                    {
                        returnList.Add(nodes[x, y]);
                    }
                }
            }
            return returnList;
        }

        public List<Node> GetListOfNodesWithEnemies()
        {
            List<Node> returnList = new List<Node>();
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < columns; y++)
                {
                    if (nodes[x, y].unitOnTile == UnitOnTile.enemy)
                    {
                        returnList.Add(nodes[x, y]);
                    }
                }
            }
            return returnList;
        }
    }
}
