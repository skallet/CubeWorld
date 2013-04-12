using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CubeWorldTrees.Map
{
    class Map
    {

        #region Parameters

        const int MAX_ALLOCATED_TREES = 10;

        protected Trees.QuadTree.QuadTree<Block> root;

        protected Rectangle space;

        protected MapGenerator generator;

        public static Mutex moveMutex = new Mutex(false, "moveMutex");

        protected static int tiles;

        public static int getTiles
        {
            get { return tiles; }
            set { }
        }

        protected int treeChaining;

        protected List<String> allocatedTrees = new List<String>(Map.MAX_ALLOCATED_TREES);

        protected Models.TilesModel model;

        #endregion

        #region Constructors

        public Map(Models.TilesModel Model, int Tiles = 256, int TreeChaining = 1)
        {
            tiles = Tiles;
            treeChaining = TreeChaining;
            model = Model;

            space = new Rectangle(0, 0, tiles);
            root = Trees.QuadTree.QuadTree<Block>.getFreeTree(space, treeChaining);
            generator = new MapGenerator(tiles, tiles);
        }

        #endregion

        #region World helpers

        protected Trees.QuadTree.QuadTree<Block> generate(int tiles)
        {
            Block block;
            generator.Generate();

            Trees.QuadTree.QuadTree<Block> quadTree = Trees.QuadTree.QuadTree<Block>.getFreeTree(new Rectangle(0, 0, tiles), 0);
            {
                for (int x = 0; x < tiles; x++)
                {
                    for (int y = 0; y < tiles; y++)
                    {
                        block = generator.GetBlock(x, y);

                        if (block != null)
                        {
                            quadTree.Insert(block);
                            //Console.WriteLine("Inserted ({0}, {1}): {2}", block.location.x, block.location.y, quadTree.Get(new Rectangle(x, y, 1)) != null);
                        }
                        else
                            Console.WriteLine("\tBLock not found!");
                    }
                }
            }

            return quadTree;
        }

        protected Block getBottomTreeBlock(Rectangle coords)
        {
            int newX, newY;
            int width, height = treeChaining;

            width = getActualNodeWidth(height - 1);

            Trees.QuadTree.QuadTree<Block> node = root, parent;
            parent = root;

            Rectangle nodeLocation = new Rectangle(coords.x / width, coords.y / width, tiles), elementLocation;
            Block bottomBlock = node.Get(nodeLocation);

            if (bottomBlock == null)
            {
                bottomBlock = new Block(1, nodeLocation);
                bottomBlock.tree = Trees.QuadTree.QuadTree<Block>.getFreeTree(new Rectangle(nodeLocation.x, nodeLocation.y, tiles), 0);
                node.Insert(bottomBlock);
            }

            //Console.WriteLine("Target: {0}, {1}, {2}, steps: {3}", coords.x, coords.y, coords.width, treeChaining - 1);
            //Console.WriteLine("Root {0}, {1}, {2}", nodeLocation.x, nodeLocation.y, nodeLocation.width);

            for (int i = 0; i < treeChaining - 1; i++)
            {
                height = treeChaining - 1 - i;
                width = getActualNodeWidth(height - 1);

                newX = (nodeLocation.x / width);
                newY = (nodeLocation.y / width);

                //Console.WriteLine("- Node {0}, {1}, {2}", newX, newY, tiles);

                elementLocation = new Rectangle(newX, newY, tiles);
                nodeLocation = new Rectangle(newX, newY, 1);

                parent = node;
                bottomBlock = node.Get(nodeLocation);

                if (bottomBlock == null)
                {
                    bottomBlock = new Block(1, new Rectangle(newX, newY, 1));
                    bottomBlock.tree = Trees.QuadTree.QuadTree<Block>.getFreeTree(elementLocation, height - 1);
                    node.Insert(bottomBlock);
                }

                node = parent.Get(nodeLocation).tree;
            }

            return bottomBlock;
        }

        protected void insetrBottomTreeBlock(Block block)
        {
            Block node = getBottomTreeBlock(block.location);
            //Console.WriteLine("Trying insert to {0}, {1} from {2}, {3}", node.location.x, node.location.y, block.location.x, block.location.y);
            node.tree.Insert(block);
        }

        public int getMapWidth()
        {
            return (int)Math.Pow(tiles, treeChaining + 1);
        }

        protected int getActualNodeWidth(int height)
        {
            return (int)Math.Pow(tiles, height);
        }

        #endregion

        #region World operations

        public Block getBlock(Rectangle coord)
        {
            Rectangle localCoord = new Rectangle(coord.x % tiles, coord.y % tiles, 1);
            Rectangle treePosition = new Rectangle(coord.x / tiles, coord.y / tiles, tiles);
            //Console.WriteLine("Searching tree X: {0}, Y: {1}", treePosition.x, treePosition.y);

            Trees.QuadTree.QuadTree<Block> tree = getTree(treePosition);
            return tree.Get(localCoord);
        }

        public Trees.QuadTree.QuadTree<Block> getTree(Rectangle coord)
        {
            Block treeBlock = getBottomTreeBlock(coord).tree.Get(coord);

            if (coord.x < 0 
                || coord.y < 0
                ||coord.x >= Math.Pow(tiles, treeChaining)
                || coord.y >= Math.Pow(tiles, treeChaining))
            {
                return null;
            }

            if (treeBlock == null || treeBlock.tree == null)
            {
                Block block = new Block(0, coord);

                if (model.treeExist(coord.x * tiles, coord.y * tiles))
                {
                    //Console.WriteLine("Loading tree...");
                    block.tree = model.loadTree(new Rectangle(coord.x, coord.y, tiles));
                }
                else
                {
                    Rectangle baseCoords = new Rectangle(coord.x * tiles, coord.y * tiles, coord.width);
                    block.tree = generate(tiles);
                    model.insertTree(block.tree, baseCoords);
                }

                treeBlock = block;
                treeBlock.location = new Rectangle(coord.x, coord.y, 1);

                insetrBottomTreeBlock(treeBlock);
            }

            return treeBlock.tree;
        }

        public Trees.QuadTree.QuadTree<Block> getIntersectTree(Rectangle coord)
        {
            Rectangle treeCoord = new Rectangle(coord.x / tiles, coord.y / tiles, tiles);
            return getTree(treeCoord);
        }

        public List<Block> getIntersect(Rectangle space)
        {
            List<Block> list = new List<Block>();
            List<Block> listTree = new List<Block>();

            Rectangle coords;
            Boolean added = false;
            Block treeBlock;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    added = false;
                    coords = new Rectangle((x * space.width + space.x) / tiles, (y * space.width + space.y) / tiles, tiles);

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
                        //Console.WriteLine("Getting tree X: {0}, Y: {1}, width: {2}, Exist: {3}", coords.x, coords.y, coords.width, treeBlock.tree != null);

                        if (treeBlock.tree != null)
                            listTree.Add(treeBlock);
                    }
                }
            }

            /*Console.WriteLine("Space {0} {1} => {2} {3}", space.x, space.y, space.x + space.width, space.y + space.width);
            Console.WriteLine("In {0} tree.", listTree.Count());

            foreach (Block i in listTree)
            {
                Console.WriteLine("== tree {0} {1}", i.location.x, i.location.y);
            }*/

            Block block;
            foreach (Block i in listTree)
            {
                for (int x = 0; x < tiles; x++)
                {
                    for (int y = 0; y < tiles; y++)
                    {
                        if (space.Contains(new Rectangle(x + i.location.x * i.location.width, y + i.location.y * i.location.width, 1))) 
                        {
                            block = i.tree.Get(new Rectangle(x, y, 1));

                            if (block != null)
                            {
                                //Console.WriteLine("Test {0} {1}", x + i.location.x * i.location.width, y + i.location.y * i.location.width);
                                list.Add(new Block(block.val, new Rectangle(x + i.location.x * i.location.width, y + i.location.y * i.location.width, 1)));
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
