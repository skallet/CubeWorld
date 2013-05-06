using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeWorldTrees.Tests
{
    class OptimalTreeSize : BaseTest
    {

        int maxTiles, maxChaining, minTiles;

        public OptimalTreeSize (int Tiles, int Chaining, int MaxTiles, int MaxChaining) 
            : base(Tiles, Chaining)
        {
            minTiles = Tiles;
            maxTiles = MaxTiles;
            maxChaining = MaxChaining;
        }

        public override void Run()
        {
            base.Run();

            Trees.QuadTree.QuadTree<Map.Block> tree;
            Map.Rectangle rectangle;

            while (tiles <= maxTiles && chaining <= maxChaining)
            {                
                model.RemoveAllTiles();

                map = new Map.Map(model, tiles, chaining);

                Console.WriteLine("Tiles = {0}, Chaining = {1}", tiles, chaining);

                Trees.QuadTree.QuadTree<Map.Block>.freeAllTree();
                rectangle = new Map.Rectangle(0, 0, 1);

                StartTimer();
                tree = map.getIntersectTree(rectangle);
                StopTimer(String.Format("Read Part + DB + storing"));

                StartTimer();
                tree = map.getIntersectTree(rectangle);
                StopTimer(String.Format("Read Part + DB"));

                StartTimer();
                tree = map.getIntersectTree(rectangle);
                StopTimer(String.Format("Read Part"));

                Trees.QuadTree.QuadTree<Map.Block>.freeAllTree();
                rectangle = new Map.Rectangle(0, 0, 15);
                List<Map.Block> parts = null;

                StartTimer();
                parts = map.getIntersect(rectangle);
                StopTimer(String.Format("Read Space + DB"));

                StartTimer();
                parts = map.getIntersect(rectangle);
                StopTimer(String.Format("Read Space"));

                tiles *= 2;
                if (tiles > maxTiles)
                {
                    tiles = minTiles;
                    chaining++;
                }
            }

            Console.WriteLine("Test finished!");
        }

    }
}
