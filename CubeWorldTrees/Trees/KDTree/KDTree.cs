using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Trees.KDTree
{
    class KDTree<T> : BaseTree where T : Map.IBlock
    {

        KDTreeNode<T> m_root;

    }
}
