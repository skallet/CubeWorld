using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CubeWorldTrees.Trees.QuadTree
{

    class QuadTree<T> : BaseTree where T : Map.IBlock
    {

        struct TreeStruct
        {
            public int lastUser;
            public Map.Rectangle space;
            public QuadTree<T> tree;
            public int treeId;
        }

        QuadTreeNode<T> m_root;

        public static Mutex mutex = new Mutex(false, "treeMutex");

        const int MAX_INSTANCES = 100;

        private static List<TreeStruct> treeList = new List<TreeStruct>(MAX_INSTANCES);

        static readonly object _locker = new object();

        public static QuadTree<T> getFreeTree(Map.Rectangle space, int treeId = 0)
        {
            //Console.WriteLine("Looking for {0} {1} {2}", space.x, space.y, space.width);
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            QuadTree<T> tree = new QuadTree<T>(space);
            Boolean found = false;
            TreeStruct ts;

            mutex.WaitOne();

            for (int i = 0; i < treeList.Count; i++)
            {
                ts = treeList[i];

                if (ts.treeId != 0 && ts.treeId == treeId)
                {
                    found = true;
                    tree = ts.tree;
                    ts.lastUser = threadId;
                    ts.space = space;

                    break;
                }
            }

            if (!found && treeList.Count < MAX_INSTANCES)
            {
                found = true;
                ts = new TreeStruct();
                ts.tree = tree;
                ts.lastUser = threadId;
                ts.space = space;
                ts.treeId = treeId;

                treeList.Add(ts);
            }

            mutex.ReleaseMutex();

            if (!found)
            {
                lock (_locker)
                    while (treeList.Count >= MAX_INSTANCES)
                        Monitor.Wait(_locker);
                
                return getFreeTree(space);
            }

            return tree;
        }

        public static void unsetAllTree()
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            TreeStruct ts;

            mutex.WaitOne();

            for (int i = 0; i < treeList.Count; i++)
            {
                ts = treeList[i];

                lock (_locker)
                {
                    treeList.Remove(ts);    
                    Monitor.Pulse(_locker);
                }
            }

            //Console.WriteLine("Space: {0}", MAX_INSTANCES - treeList.Count);

            mutex.ReleaseMutex();
        }

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

        public void Dump()
        {
            m_root.Dump();
        }

        public int DumpCount()
        {
            return m_root.DumpCount();
        }

        public int[,] GetSpace(Map.Rectangle position, int depth, out Map.Rectangle zero)
        {
            zero = null;

            if (depth < 1 || depth > 8)
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
