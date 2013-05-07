using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    class MapGenerator
    {

        #region Parameters

        private int _width;
        private int _height;
        private bool _isGenerated;
        private Block[,] _world;

        #endregion Parameters

        #region Getters and Setters

        public int width
        {
            get { return _width; }
            set { _width = (value > 0) ? value : 1; }
        }

        public int height
        {
            get { return _height; }
            set { _height = (value > 0) ? value : 1; }
        }

        public bool isGenerated
        {
            get { return _isGenerated; }
            set { }
        }

        public Block GetBlock(int x, int y)
        {
            if (isGenerated 
                && x >= 0 
                && x < width 
                && y < height 
                && y >= 0)
            {
                return _world[x, y];
            }

            return null;
        }

        #endregion Getters and Setters

        #region Constructors

        public MapGenerator()
        {
        }

        public MapGenerator(int Width, int Height)
        {
            width = Width;
            height = Height;
            _isGenerated = false;
        }

        #endregion Constructors

        public void Generate()
        {
            _world = new Block[width, height];
            Random rand = new Random();
            int value;
            Rectangle location = new Rectangle(0, 0, 1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    location.x = x;
                    location.y = y;
                    
                    value = rand.Next(1, 5);
                    _world[x, y] = new Block(value, location);
                }
            }

            _isGenerated = true;
        }

    }
}
