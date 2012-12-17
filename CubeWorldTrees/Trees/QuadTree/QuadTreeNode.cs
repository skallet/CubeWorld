using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Trees.QuadTree
{
    class QuadTreeNode<T> where T : Map.IBlock
    {

        private Map.Rectangle m_bounds;

        public QuadTreeNode(Map.Rectangle Bounds)
        {
            m_bounds = Bounds;

            if (m_bounds.width <= 10 && m_bounds.width >= 2)
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

            if (m_bounds.width == 1)
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

        public T Get(Map.Rectangle rect)
        {
            if (!m_bounds.Contains(rect))
                return default(T);

            if (m_bounds.Equals(rect))
                return m_block;

            foreach (QuadTreeNode<T> node in m_nodes)
            {
                if (node.m_bounds.Contains(rect))
                {
                    return node.Get(rect);
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
