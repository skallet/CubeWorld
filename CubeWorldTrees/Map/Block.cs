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

        private int _player;

        private Boolean _change;

        private Rectangle _location;

        private Trees.QuadTree.QuadTree<Block> _tree;

        #endregion Parameters


        #region Getters and Setters

        public int val
        {
            get { return _val; }
            set { _val = value; }
        }

        public Boolean change
        {
            get { return _change; }
            set { _change = true; }
        }

        public void update()
        {
            change = true;
        }

        public int player
        {
            get { return _player; }
            set { _player = value; }
        }

        public Rectangle location
        {
            get { return _location; }
            set {
                Rectangle loc = value as Rectangle;
                _location = new Rectangle(loc.x, loc.y, loc.width); 
            }
        }

        public Trees.QuadTree.QuadTree<Block> tree
        {
            get { return _tree; }
            set { _tree = value; }
        }

        public Boolean isSolid()
        {
            return (val == 4);
        }

        #endregion Getters and Setters


        #region Constructors

        public Block(int Value, Rectangle Location)
        {
            val = Value;
            location = Location;
            player = 0;
            _change = false;
        }

        #endregion
    }
}
