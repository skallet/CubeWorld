using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Trees.QuadTree
{
    class QuadTree<T> : BaseTree where T : Map.IBlock
    {

        QuadTreeNode<T> m_root;

        public QuadTree(Map.Rectangle space) 
        {
            m_root = new QuadTreeNode<T>(space);
        }

        public void Insert(T item)
        {
            m_root.Insert(item);
        }

        public T Get(Map.Rectangle rect)
        {
            return m_root.Get(rect);
        }

        public int[,] GetSpace(Map.Rectangle position, int depth, out Map.Rectangle zero)
        {
            zero = null;

            if (depth < 1 || depth > 5)
                return null;

            int width = (int)Math.Pow(2, depth);
            int[,] space = new int[width, width];

            //Console.WriteLine("Attempt to {0:D} {1:D} from {2:D} {3:D} of width {4:D}", position.x - (position.x % width), position.y - (position.y % width), position.x, position.y, width);
            zero = new Map.Rectangle(position.x - (position.x % width), position.y - (position.y % width), width);
            QuadTreeNode<T> node = m_root.GetNode(zero);

            //test
            Random rand = new Random();

            if (node != null)
            {
                int pointer = 0;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < width; y++)
                    {
                        pointer = x * width + y;
                        //space[x, y] = rand.Next(1, 3);
                        space[x, y] = node.GetPart(pointer);
                    }
                }

                return space;
            }

            return null;
        }

    }
}
