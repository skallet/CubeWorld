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
            Trees.QuadTree.QuadTree<Block>.model = model;
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

        Mutex initMutex = new Mutex(false, "searchTree");

        protected Block getBottomTreeBlock(Rectangle coords)
        {
            int newX = coords.x, newY = coords.y;
            int oldWidth, width, height = treeChaining;

            width = getActualNodeWidth(height);
            Trees.QuadTree.QuadTree<Block> node = root, parent = null;
            Rectangle location = null;

            Block bottomBlock = null;

            //Console.WriteLine("==");

            initMutex.WaitOne();
            while (height > 0)
            {
                parent = node;
                height--;

                oldWidth = width;
                width = getActualNodeWidth(height);

                location = new Rectangle(newX / width, newY / width, oldWidth);
                //Console.WriteLine("MTree {0} {1} {2}", location.x, location.y, location.width);

                newX = newX % width;
                newY = newY % width;

                bottomBlock = node.Get(location);

                if (bottomBlock == null)
                {
                    bottomBlock = new Block(1, new Rectangle(location.x, location.y, oldWidth));
                    bottomBlock.tree = Trees.QuadTree.QuadTree<Block>.getFreeTree(location, height);
                    node.Insert(bottomBlock);
                }

                node = bottomBlock.tree;
            }
            initMutex.ReleaseMutex();

            return bottomBlock;
        }

        protected void insetrBottomTreeBlock(Block block)
        {
            int newX = block.location.x, newY = block.location.y;
            int oldWidth, width, height = treeChaining;

            width = getActualNodeWidth(height);
            Trees.QuadTree.QuadTree<Block> node = root, parent;
            parent = node;
            Rectangle location = null;

            Block bottomBlock = null;

            initMutex.WaitOne();
            while (height > 0)
            {
                parent = node;
                height--;

                oldWidth = width;
                width = getActualNodeWidth(height);

                location = new Rectangle(newX / width, newY / width, oldWidth);

                newX = newX % width;
                newY = newY % width;

                bottomBlock = node.Get(location);

                if (bottomBlock == null)
                {
                    bottomBlock = new Block(1, new Rectangle(location.x, location.y, oldWidth));
                    bottomBlock.tree = Trees.QuadTree.QuadTree<Block>.getFreeTree(location, height);
                    node.Insert(bottomBlock);
                }

                node = bottomBlock.tree;
            }
            initMutex.ReleaseMutex();

            if (parent != null)
            {
                block.location = location;
                block.location.width = tiles;

                parent.Insert(block);
            }
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

        Mutex getMutex = new Mutex(false, "getTree");

        public Trees.QuadTree.QuadTree<Block> getTree(Rectangle coord)
        {
            //Console.WriteLine("Tree: {0} {1}", coord.x, coord.y);
            Block treeBlock = getBottomTreeBlock(coord);

            if (coord.x < 0 
                || coord.y < 0
                ||coord.x >= Math.Pow(tiles, treeChaining - 1)
                || coord.y >= Math.Pow(tiles, treeChaining - 1))
            {
                return null;
            }

            getMutex.WaitOne();
            if (treeBlock == null || treeBlock.tree == null || treeBlock.tree.DumpCount() == 0)
            {
                Block block = new Block(0, coord);

                if (model.treeExist(coord.x * tiles, coord.y * tiles))
                {
                    //Console.WriteLine("Loading tree...");
                    treeBlock.tree = model.loadTree(new Rectangle(coord.x, coord.y, tiles));
                }
                else
                {
                    //Console.WriteLine("Creating tree...");
                    Rectangle baseCoords = new Rectangle(coord.x * tiles, coord.y * tiles, tiles);
                    block.tree = generate(tiles);

                    model.insertTree(block.tree, baseCoords);
                    treeBlock.tree = model.loadTree(new Rectangle(coord.x, coord.y, tiles));
                }
            }
            getMutex.ReleaseMutex();

            //Console.WriteLine("DUMP {0}", treeBlock.tree.DumpCount());
            return treeBlock.tree;
        }

        public Trees.QuadTree.QuadTree<Block> getIntersectTree(Rectangle coord)
        {
            if (coord.x < 0 || coord.y < 0)
                return null;

            Rectangle treeCoord = new Rectangle(coord.x / tiles, coord.y / tiles, tiles);
            return getTree(treeCoord);
        }

        Mutex intersectMutex = new Mutex(false, "intersect");

        public List<Block> getIntersect(Rectangle space)
        {
            List<Block> list = new List<Block>();
            List<Block> listTree = new List<Block>();

            Rectangle coords;
            Boolean added = false;
            Block treeBlock;

            intersectMutex.WaitOne();
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

                        if (treeBlock.tree != null)
                            listTree.Add(treeBlock);
                    }
                }
            }
            intersectMutex.ReleaseMutex();

            /*Console.WriteLine("Space {0} {1} => {2} {3}", space.x, space.y, space.x + space.width, space.y + space.width);
            Console.WriteLine("In {0} tree.", listTree.Count());

            foreach (Block i in listTree)
            {
                Console.WriteLine("== tree {0} {1}", i.location.x, i.location.y);
            }*/

            Block block;
            int bx, by;
            foreach (Block i in listTree)
            {
                //Console.WriteLine("Saving tree {0} {1}", i.location.x, i.location.y);
                for (int x = 0; x < tiles; x++)
                {
                    for (int y = 0; y < tiles; y++)
                    {
                        if (space.Contains(new Rectangle(x + i.location.x * i.location.width, y + i.location.y * i.location.width, 1))) 
                        {
                            block = i.tree.Get(new Rectangle(x, y, 1));

                            if (block != null)
                            {
                                bx = block.location.x + i.location.x * i.location.width;
                                by = block.location.y + i.location.y * i.location.width;

                                list.Add(new Block(block.val, new Rectangle(bx, by, 1)));
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
