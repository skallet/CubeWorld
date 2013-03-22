using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    class Map
    {

        #region Parameters

        protected Trees.QuadTree.QuadTree<Block> root;

        protected Rectangle space;

        protected MapGenerator generator;

        protected int tyles;

        #endregion

        #region Constructors

        public Map(int Tyles = 256)
        {
            tyles = Tyles;
            space = new Rectangle(0, 0, tyles);
            root = new Trees.QuadTree.QuadTree<Block>(space);
            generator = new MapGenerator(tyles, tyles);
        }

        #endregion

        #region World helpers

        protected Trees.QuadTree.QuadTree<Block> generate(int tyles)
        {
            Block block;
            generator.Generate();

            Trees.QuadTree.QuadTree<Block> quadTree = new Trees.QuadTree.QuadTree<Block>(new Rectangle(0, 0, tyles));
            {
                for (int x = 0; x < tyles; x++)
                {
                    for (int y = 0; y < tyles; y++)
                    {
                        block = generator.GetBlock(x, y);
                        if (block != null)
                            quadTree.Insert(block);
                        else
                            Console.WriteLine("\tBLock not found!");
                    }
                }
            }

            return quadTree;
        }

        #endregion

        #region World operations

        public Block getBlock(Rectangle coord)
        {
            Rectangle localCoord = new Rectangle(coord.x % 256, coord.y % 256, 1);
            Rectangle treePosition = new Rectangle(coord.x / tyles, coord.y / tyles, 1);
            //Console.WriteLine("Searching tree X: {0}, Y: {1}", treePosition.x, treePosition.y);

            Block treeBlock = root.Get(treePosition);

            if (treeBlock == null || treeBlock.tree == null)
            {
                //Console.WriteLine("Creating new tree!");                
                Block block = new Block(0, treePosition);
                block.tree = generate(tyles);
                root.Insert(block);
            }
            else
            {
                //Console.WriteLine("Existing tree!");
            }

            treeBlock = root.Get(treePosition);
            return treeBlock.tree.Get(localCoord);
        }

        public Trees.QuadTree.QuadTree<Block> getTree(Rectangle coord)
        {
            Block treeBlock = root.Get(coord);

            if (treeBlock == null || treeBlock.tree == null)
            {
                Block block = new Block(0, coord);
                block.tree = generate(tyles);
                root.Insert(block);
            }

            return root.Get(coord).tree;
        }

        public List<Block> getIntersect(Rectangle space)
        {
            List<Block> list = new List<Block>();
            List<Block> listTree = new List<Block>();
            //Console.WriteLine("Getting intersect X: {0}, Y: {1}, XD: {2}, YD: {3}", space.x, space.y, space.x + space.width, space.y + space.width);

            Rectangle coords;
            Boolean added = false;
            Block treeBlock;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    added = false;
                    coords = new Rectangle((x * space.width + space.x) / tyles, (y * space.width + space.y) / tyles, 1);

                    foreach (Block i in listTree)
                    {
                        if (i.location.x == coords.x && i.location.y == coords.y)
                        {
                            added = true;
                        }
                    }

                    if (!added)
                    {
                        treeBlock = new Block(listTree.Count, coords);
                        treeBlock.tree = getTree(coords);

                        if (treeBlock.tree != null)
                            listTree.Add(treeBlock);
                    }
                }
            }

            Rectangle zero;
            foreach (Block i in listTree)
            {
                int[,] parts = i.tree.GetSpace(new Rectangle(0, 0, tyles), 8, out zero);
                if (parts != null)
                {
                    for (int x = 0; x < parts.GetLength(0); x++)
                    {
                        for (int y = 0; y < parts.GetLength(1); y++)
                        {
                            if (space.Contains(new Rectangle(x, y, 1))) 
                            {
                                list.Add(new Block(parts[x, y], new Rectangle(x + i.location.x, y + i.location.y, 1)));
                            }
                        }
                    }
                }
            }

            return list;
        }
        

        #endregion

    }
}
