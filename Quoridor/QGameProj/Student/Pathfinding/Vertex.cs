using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

class Vertex
{
    public List<Vertex> Neighbours { get; private set; }
    public List<Edge> Edges { get; private set; }

    public Point Position { get; private set; }

    public bool IsVisited { get; set; }
    public float Cost { get; set; }

    public int NeighbourCount => Neighbours.Count;

    public Vertex(Point pos)
    {
        Neighbours = new List<Vertex>();
        Edges = new List<Edge>();

        Position = pos;

        IsVisited = false;
        Cost = float.PositiveInfinity;
    }

    public float EdgeWeight()
    {
        float total = 0;
        for (int i = 0; i < Edges.Count; i++)
        {
            total += Edges[i].Weight;
        }
        return total;
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
