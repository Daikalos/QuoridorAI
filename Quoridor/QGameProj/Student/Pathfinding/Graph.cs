using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

class Graph
{
    public List<Vertex> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }

    public int VerticesCount => Vertices.Count;
    public int EdgesCount => Edges.Count;

    private SpelBräde board;

    public Graph(SpelBräde board)
    {
        this.board = board;

        Edges = new List<Edge>();
        Vertices = new List<Vertex>();

        GenerateGraph();
    }

    private void GenerateGraph()
    {
        int boardLengthX = board.horisontellaLångaVäggar.GetLength(0) + 1;
        int boardLengthY = board.horisontellaLångaVäggar.GetLength(1) + 1;

        int wallLengthX = board.horisontellaLångaVäggar.GetLength(0);
        int wallLengthY = board.horisontellaLångaVäggar.GetLength(1);
        
        for (int y = 0; y < boardLengthY; y++) // Add all vertices
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                Vertices.Add(new Vertex(new Point(x, y)));
            }
        }

        for (int y = 0; y < boardLengthY; y++) // Add all edges
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                for (int i = -1; i <= 1; i += 2) // Left to Right
                {
                    if (WithinBounds((x + i), y, boardLengthX, boardLengthY))
                    {
                        Vertex vertex = AtPos(new Point(x, y), boardLengthX);
                        Vertex neighbour = AtPos(new Point(x + i, y), boardLengthX);

                        int xPos = (wallLengthX - 1) - x;
                        int yPos = y;

                        // It just works
                        if (i < 0 && x > 0) xPos++;
                        if (y > 0) yPos--;

                        bool addEdge = true;
                        for (int l = 0; l < 2; l++) // Length of individual wall
                        {
                            if (WithinBounds(xPos, yPos + l, wallLengthX, wallLengthY))
                            {
                                addEdge = !board.vertikalaLångaVäggar[xPos, yPos + l];
                            }

                            if (!addEdge) break;
                        }

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour, 1));
                    }
                }
                for (int j = -1; j <= 1; j += 2) //Top to Bottom
                {
                    if (WithinBounds(x, (y + j), boardLengthX, boardLengthY))
                    {
                        Vertex vertex = AtPos(new Point(x, y), boardLengthX);
                        Vertex neighbour = AtPos(new Point(x, y + j), boardLengthX);

                        int xPos = (wallLengthX - 1) - x;
                        int yPos = y;

                        if (j < 0 && y > 0) yPos--;

                        bool addEdge = true;
                        for (int l = 0; l < 2; l++) // Length of individual wall
                        {
                            if (WithinBounds(xPos + l, yPos, wallLengthX, wallLengthY))
                            {
                                addEdge = !board.horisontellaLångaVäggar[xPos + l, yPos];
                            }

                            if (!addEdge) break;
                        }

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour, 1));
                    }
                }
            }
        }

        for (int y = 0; y < boardLengthY; y++) // Add all vertices
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                Console.Write("[" + x + ", " + y + "] " + AtPos(new Point(x, y), boardLengthX).EdgeWeight() + " ");
            }
            Console.WriteLine("");
        }
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }

    public float DistanceTo(Vertex from, Vertex to)
    {
        return (float)Math.Sqrt(
            Math.Pow(from.Position.X - to.Position.X, 2) + 
            Math.Pow(from.Position.Y - to.Position.Y, 2));
    }

    private Vertex AtPos(Point pos, int width)
    {
        return Vertices[pos.X + pos.Y * width];
    }

    private bool WithinBounds(int x, int y, float boundX, float boundY)
    {
        return !(x < 0 || y < 0 || x >= boundX || y >= boundY);
    }
}
