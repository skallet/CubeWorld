using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    interface IBlock
    {

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

    }
}
