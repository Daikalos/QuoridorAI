using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

class Vertex
{
    public List<Vertex> Neighbours { get; private set; }
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

        Neighbours = new List<Vertex>();
        Edges = new List<Edge>();

        IsVisited = false;

        G = float.MaxValue;
        H = float.MaxValue;
    }

    public void AddNeighbour(Vertex vertex)
    {
        Neighbours.Add(vertex);
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }
}
