using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class Vertex
{
    public List<Vertex> Neighbours => Edges.Select(e => e.To).ToList();
    public List<Edge> Edges { get; private set; }

    public Vertex Parent { get; set; }
    public Point Position { get; private set; }

    public bool IsVisited { get; set; }

    public float F => G + H;     // Total Cost
    public float G { get; set; } // Distance from current to start
    public float H { get; set; } // Distance from current to end

    public int EdgeCount => Edges.Count;

    public Vertex(Point pos)
    {
        Position = pos;

        Edges = new List<Edge>();

        IsVisited = false;

        G = float.PositiveInfinity;
        H = float.PositiveInfinity;
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }

    public void RemoveEdge(Edge edge)
    {
        Edges.Remove(edge);
    }
}
