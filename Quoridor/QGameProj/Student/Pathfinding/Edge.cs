using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

class Edge
{
    public Vertex From { get; private set; }
    public Vertex To { get; private set; }

    public float Weight { get; private set; }

    public Edge(Vertex from, Vertex to, float weight)
    {
        From = from;
        To = to;
        Weight = weight;

        from.AddNeighbour(to);
        from.AddEdge(this);
    }
}
