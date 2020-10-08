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

    public Vertex(Point pos)
    {
        Position = pos;

        Neighbours = new List<Vertex>();
        Edges = new List<Edge>();

        IsVisited = false;

        G = float.MaxValue;
        H = float.MaxValue;
    }

    /*
    public void GetDir()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < Edges.Count)
            {
                Point off = Edges[i].To.Position - Edges[i].From.Position;
                if (off.X == 1) Console.Write("L");
                if (off.X == -1) Console.Write("R");
                if (off.Y == -1) Console.Write("U");
                if (off.Y == 1) Console.Write("D");
            }
            else
            {
                Console.Write(" ");
            }
        }
    }*/

    public void AddNeighbour(Vertex vertex)
    {
        Neighbours.Add(vertex);
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }
}
