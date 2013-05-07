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
            public List<int> threadUsers;
            public Map.Rectangle space;
            public QuadTree<T> tree;
            public int height;
        }

        QuadTreeNode<T> m_root;

        private Mutex updateMutex;

        public static Mutex mutex = new Mutex(false, "treeMutex");

        public static Models.TilesModel model;

        const int MAX_INSTANCES = 100;

        private static List<TreeStruct> treeList = new List<TreeStruct>(MAX_INSTANCES);

        static readonly object _locker = new object();

        public static QuadTree<T> getFreeTree(Map.Rectangle space, int height)
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            QuadTree<T> tree = new QuadTree<T>(new Map.Rectangle(0, 0, space.width));
            Boolean found = false;
            TreeStruct ts;

            mutex.WaitOne();

            //Console.WriteLine("Search {0} {1} {2}", space.x, space.y, height);

            for (int i = 0; i < treeList.Count(); i++)
            {
                ts = treeList[i];

                if (ts.height == height
                    && ts.space.Equals(space))
                {
                    //Console.WriteLine("Getting existing tree! {0}", height);
                    found = true;
                    tree = ts.tree;

                    if (!ts.threadUsers.Contains(threadId))
                        ts.threadUsers.Add(threadId);

                    break;
                }
                else if (treeList.Count() >= MAX_INSTANCES 
                    && ts.threadUsers.Count() == 0)
                {
                    //Console.WriteLine("Rewriting tree! {0}", height);
                    if (ts.height == 0)
                        ts.tree.Store(ts.space);
                    treeList.Remove(ts);
                    break;
                }
            }

            if (!found && treeList.Count() < MAX_INSTANCES)
            {
                //Console.WriteLine("Creating new tree! {0}", height);
                found = true;
                ts = new TreeStruct();
                ts.tree = tree;
                ts.threadUsers = new List<int>();
                ts.threadUsers.Add(threadId);
                ts.space = space;
                ts.height = height;

                treeList.Add(ts);
            }

            mutex.ReleaseMutex();

            if (!found)
            {
                System.Diagnostics.Debug.WriteLine("Wait for memory!");
                lock (_locker)
                {
                    Monitor.Wait(_locker);
                }
                System.Diagnostics.Debug.WriteLine("Memory available!");
                
                return getFreeTree(space, height);
            }

            return tree;
        }

        public static void freeAllTree()
        {
            TreeStruct ts;
            mutex.WaitOne();

            for (int i = 0; i < treeList.Count(); i++)
            {
                ts = treeList[i];
                if (ts.height == 0)
                    ts.tree.Store(ts.space);
                treeList.Remove(ts);
            }

            mutex.ReleaseMutex();
        }

        public static void unsetAllTree()
        {
            int threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            TreeStruct ts;

            mutex.WaitOne();

            for (int i = 0; i < treeList.Count(); i++)
            {
                ts = treeList[i];

                if (ts.threadUsers.Contains(threadId))
                {
                    ts.threadUsers.Remove(threadId);

                    if (ts.threadUsers.Count() == 0)
                    {
                        lock (_locker)
                        {
                            Monitor.PulseAll(_locker);
                        }
                    }
                }
            }

            mutex.ReleaseMutex();
        }

        private QuadTree(Map.Rectangle space) 
        {
            m_root = new QuadTreeNode<T>(space);
            updateMutex = new Mutex(false);
        }

        public void Store(Map.Rectangle space)
        {
            if (model != null)
            {
                T block;
                Map.Rectangle loc = new Map.Rectangle(0, 0, 1);
                for (int x = 0; x < m_root.getTiles(); x++)
                {
                    for (int y = 0; y < m_root.getTiles(); y++)
                    {
                        loc.x = x;
                        loc.y = y;
                        block = Get(loc);

                        if (block != null && block.change == true)
                        {
                            model.updateTile(x + space.x * m_root.getTiles(), y + space.y * m_root.getTiles(), block.player, block.val);
                        }
                    }
                }
            }
        }

        public Boolean Update(T item, int user, int changeFrom)
        {
            Boolean updated = false;
            updateMutex.WaitOne();

            T oldItem = Get(item.location);
            //Console.WriteLine("Try updated x:{0} y:{1} player:{2} value:{3}", oldItem.location.x, oldItem.location.y, oldItem.player, oldItem.val);

            if (oldItem != null)
            {
                if (oldItem.player == user
                    || oldItem.player == 0)
                {
                    if (oldItem.val == changeFrom)
                    {
                        oldItem.player = user;
                        oldItem.val = item.val;
                        oldItem.update();

                        Insert(oldItem);
                        updated = true;

                        //Console.WriteLine("Updated to x:{0} y:{1} player:{2} value:{3}", oldItem.location.x, oldItem.location.y, oldItem.player, oldItem.val);
                        oldItem = Get(item.location);
                        //Console.WriteLine("Test x:{0} y:{1} player:{2} value:{3}", oldItem.location.x, oldItem.location.y, oldItem.player, oldItem.val);
                    }
                }
            }

            updateMutex.ReleaseMutex();

            return updated;
        }

        public void Insert(T item)
        {
            m_root.Insert(item);
        }

        public T Get(Map.Rectangle rect)
        {
            Map.Rectangle local = new Map.Rectangle(rect.x % m_root.getTiles(), rect.y % m_root.getTiles(), rect.width);
            return m_root.Get(local);
        }

        public void Dump()
        {
            m_root.DumpSpace();
            //m_root.Dump();
        }

        public int DumpCount()
        {
            return m_root.DumpCount();
        }

    }
}
