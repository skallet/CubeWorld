using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Trees.QuadTree
{
    class QuadTreeNode<T> where T : Map.IBlock
    {

        private Map.Rectangle m_bounds;

        public QuadTreeNode(Map.Rectangle Bounds, bool partitioning = true)
        {
            m_bounds = Bounds;

            if (m_bounds.width >= 200 && partitioning)
                m_parts = new int[m_bounds.width * m_bounds.width];
        }

        List<QuadTreeNode<T>> m_nodes = new List<QuadTreeNode<T>>(4);
        int[] m_parts;

        private T m_block;

        public bool IsEmpty { get { return m_nodes.Count == 0; } }

        public void Insert(T item)
        {
            if (!m_bounds.Contains(item.location))
            {
                return;
            }

            if (m_nodes.Count == 0)
                CreateSubNodes();

            if (m_bounds.width == 1 || m_bounds.Equals(item.location))
            {
                m_block = item;
                return;
            }

            if (m_parts != null)
            {
                int partsKey = (item.location.x - m_bounds.x) * m_bounds.width + (item.location.y - m_bounds.y);
                m_parts[partsKey] = item.val;
            }

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                if (node.m_bounds.Contains(item.location))
                {
                    node.Insert(item);
                    return;
                }
            }

            return;
        }

        public void Dump()
        {
            if (m_block != null)
            {
                //Console.WriteLine("DUMP: {0}, {1}", m_bounds.x, m_bounds.y);
                Console.WriteLine("DUMP Block: {0}, {1} => {2}", m_bounds.x, m_bounds.y, m_block.val);
            }

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                node.Dump();
            }
        }

        public int DumpCount()
        {
            int count = 0;

            if (m_bounds.width == 1 && m_block != null)
            {
                count += 1;
            }

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                count += node.DumpCount();
            }

            return count;
        }

        public T Get(Map.Rectangle rect, int level = 1)
        {
            int maxLevel = (int)Math.Log((double)m_bounds.width, 2.0);
            //Console.WriteLine("-- {0}", level);
            //Console.WriteLine("-- Space {0} {1} {2}", m_bounds.x, m_bounds.y, m_bounds.width);

            if (!m_bounds.Contains(rect))
                return default(T);

            if (m_block != null && (m_bounds.Equals(rect) || (m_bounds.Contains(rect) && maxLevel == level)))
                return m_block;

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                if (node.m_bounds.Contains(rect))
                {
                    return node.Get(rect, level + 1);
                }
            }

            return default(T);
        }

        public QuadTreeNode<T> GetNode(Map.Rectangle rect)
        {
            if (!m_bounds.Contains(rect))
                return null;

            if (m_bounds.Equals(rect))
                return this;

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                if (node.m_bounds.Contains(rect))
                {
                    return node.GetNode(rect);
                }
            }

            return null;
        }

        private void CreateSubNodes()
        {
            if (m_bounds.width == 1)
                return;

            int halfWidth = (m_bounds.width / 2);

            m_nodes.Add(new QuadTreeNode<T>(new Map.Rectangle(m_bounds.x, m_bounds.y, halfWidth)));
            m_nodes.Add(new QuadTreeNode<T>(new Map.Rectangle(m_bounds.x, m_bounds.y + halfWidth, halfWidth)));
            m_nodes.Add(new QuadTreeNode<T>(new Map.Rectangle(m_bounds.x + halfWidth, m_bounds.y + halfWidth, halfWidth)));
            m_nodes.Add(new QuadTreeNode<T>(new Map.Rectangle(m_bounds.x + halfWidth, m_bounds.y, halfWidth)));
        }

        public int GetPart(int key)
        {
            if (m_parts == null || key < 0 || key > m_bounds.width * m_bounds.width)
                return 0;
            
            return m_parts[key];
        }

    }
}
