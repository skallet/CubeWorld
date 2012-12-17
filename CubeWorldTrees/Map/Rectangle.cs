using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Map
{
    class Rectangle
    {

        #region Parameters

        private int _width;

        private int _x;
        private int _y;

        #endregion Parameters

        #region Getters and Setters

        public int width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int x
        {
            get { return _x; }
            set { _x = value; }
        }

        public int y
        {
            get { return _y; }
            set { _y = value; }
        }

        #endregion Getters and Setters

        #region Constructors

        public Rectangle(int X, int Y, int Width)
        {
            x = X;
            y = Y;
            width = Width;
        }

        #endregion

        public bool Contains(Rectangle rect)
        {
            if ((x  <= rect.x )
                && (x + width >= rect.x + rect.width)
                && (y <= rect.y )
                && (y + width >= rect.y + rect.width))
            {
                return true;
            }

            return false;
        }

        public bool Equals(Rectangle rect)
        {
            if (x == rect.x
                && y == rect.y
                && width == rect.width)
                return true;

            return false;
        }

    }
}
