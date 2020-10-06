using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

class Graph
{
    SpelBräde board;
    List<Edge> edges;
    List<Vertex> vertex;

    public Graph(SpelBräde board)
    {
        this.board = board;

        edges = new List<Edge>();
        vertex = new List<Vertex>();

        GenerateGraph();
    }

    private void GenerateGraph()
    {
        int N = board.horisontellaLångaVäggar.GetLength(0) + 1;
        for (int x = 0; x < N; x++)
        {
            for (int y = 0; y < N; y++)
            {
                
            }
        }
    }

    public void AddEdge(Edge edge)
    {
        edges.Add(edge);
    }

    public int V()
    {
        return vertex.Count;
    }

    public int E()
    {
        return edges.Count;
    }
}
