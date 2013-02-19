using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

namespace CubeWorldTrees.Controlers
{
    class MapControler : BaseControler
    {

        protected Trees.QuadTree.QuadTree<Map.Block> quadTree;

        public MapControler(HttpListenerContext Context, Trees.QuadTree.QuadTree<Map.Block> QuadTree)
            : base(Context)
        {
            quadTree = QuadTree;
        }

        public override void BeforeRender()
        {
            base.BeforeRender();

            template.scripts.Add("/content/js/main.js");
        }

        public override void Render()
        {
            base.Render();
        }

    }
}
