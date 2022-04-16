using UnityEngine;

namespace Completed
{
    public enum ObjectOnTile
    {
        innerWall,
        outerWall,
        food,
        soda,
        exit,
        none
    }

    public enum UnitOnTile
    {
        player,
        enemy,
        none
    }

    public class Node
    {
        public int row;
        public int column;
        public ObjectOnTile objectOnTile;
        public UnitOnTile unitOnTile;
        public GameObject BlockingLayerObject;
        public GameObject foodObject;
        public Wall innerWall;

        public Node(int row, int column)
        {
            this.row = row;
            this.column = column;
            this.objectOnTile = ObjectOnTile.none;
            this.unitOnTile = UnitOnTile.none;
        }
    }
}
