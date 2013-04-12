﻿using System;
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

        private Rectangle _location;

        private Trees.QuadTree.QuadTree<Block> _tree;

        #endregion Parameters


        #region Getters and Setters

        public int val
        {
            get { return _val; }
            set { _val = value; }
        }

        public int player
        {
            get { return _player; }
            set { _player = value; }
        }

        public Rectangle location
        {
            get { return _location; }
            set { _location = value; }
        }

        public Trees.QuadTree.QuadTree<Block> tree
        {
            get { return _tree; }
            set { _tree = value; }
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
