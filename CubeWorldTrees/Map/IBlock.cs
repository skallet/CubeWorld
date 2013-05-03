using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    interface IBlock
    {

        Trees.QuadTree.QuadTree<Block> tree
        {
            get;
            set;
        }

        int val
        {
            get;
            set;
        }

        Rectangle location
        {
            get;
            set;
        }

        Boolean change
        {
            get;
            set;
        }

        void update();

        int player
        {
            get;
            set;
        }

    }
}
