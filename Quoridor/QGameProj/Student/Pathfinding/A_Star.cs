using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class A_Star : Pathfinder
{
    private Graph graph;

    public A_Star(SpelBräde bräde)
    {
        graph = new Graph(bräde);
    }

    public List<Vertex> pathTo(Vertex start, Vertex end)
    {
        List<Vertex> path = new List<Vertex>();



        return path;
    }
}
