using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

class Edge
{
    private Vertex from;
    private Vertex to;

    private float weight;

    public Edge(Vertex from, Vertex to, float weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
    }
}
