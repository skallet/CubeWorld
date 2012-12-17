using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    class Block : IBlock
    {

        #region Parameters

        private int _val;

        private Rectangle _location;

        #endregion Parameters


        #region Getters and Setters

        public int val
        {
            get { return _val; }
            set { _val = value; }
        }

        public Rectangle location
        {
            get { return _location; }
            set { _location = value; }
        }

        #endregion Getters and Setters


        #region Constructors

        public Block(int Value, Rectangle Location)
        {
            val = Value;
            location = Location;
        }

        #endregion
    }
}
