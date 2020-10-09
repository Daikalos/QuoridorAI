using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

class Graph
{
    public List<Vertex> Vertices { get; private set; }
    public List<Edge> Edges { get; private set; }

    private SpelBräde board;

    int boardLengthX;
    int boardLengthY;

    int wallLengthX;
    int wallLengthY;

    public Graph(SpelBräde board)
    {
        this.board = board;

        Edges = new List<Edge>();
        Vertices = new List<Vertex>();

        boardLengthX = board.horisontellaLångaVäggar.GetLength(0) + 1;
        boardLengthY = board.horisontellaLångaVäggar.GetLength(1) + 1;

        wallLengthX = board.horisontellaLångaVäggar.GetLength(0);
        wallLengthY = board.horisontellaLångaVäggar.GetLength(1);

        GenerateGraph();
    }

    public Vertex AtPos(int x, int y)
    {
        return Vertices[x + y * boardLengthX];
    }

    public Vertex AtPos(Point pos)
    {
        return Vertices[pos.X + pos.Y * boardLengthX];
    }

    private void GenerateGraph()
    {
        AddVertices();
        AddEdges();

        /*
        for (int y = 0; y < boardLengthY; y++)
        {
            for (int x = boardLengthX - 1; x >= 0; x--)
            {
                Console.Write("[" + x + ", " + y + "] ");
                AtPos(x, y).GetDir();
                Console.Write(" ");
            }
            Console.WriteLine("");
        }
        Console.WriteLine("");
        for (int y = 0; y < wallLengthY; y++)
        {
            for (int x = wallLengthX - 1; x >= 0; x--)
            {
                Console.Write("[" + x + ", " + y + "] " + board.horisontellaLångaVäggar[x, y]);
            }
            Console.WriteLine("");
        }*/
    }

    private void AddVertices()
    {
        for (int y = 0; y < boardLengthY; y++) // Add all vertices
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                Vertices.Add(new Vertex(new Point(x, y)));
            }
        }
    }

    private void AddEdges()
    {
        for (int y = 0; y < boardLengthY; y++) // Add all edges
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                for (int i = -1; i <= 1; i += 2) // Left and Right
                {
                    if (WithinBounds((x + i), y, boardLengthX, boardLengthY))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x + i, y);

                        int xPos = x;
                        int yPos = y - 1;

                        if (i < 0 && x > 0) xPos--;

                        bool addEdge = true; // Don't add edge if wall between vertices
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
                for (int j = -1; j <= 1; j += 2) //Top and Bottom
                {
                    if (WithinBounds(x, (y + j), boardLengthX, boardLengthY))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x, y + j);

                        int xPos = x - 1;
                        int yPos = y;

                        if (j < 0 && y > 0) yPos--;

                        bool addEdge = true; // Don't add edge if wall between vertices
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
    }

    public void ExamineBoardState()
    {
        for (int y = 0; y < boardLengthY; y++) // Add all edges
        {
            for (int x = 0; x < boardLengthX; x++)
            {
                foreach(Edge edge in AtPos(x, y).Edges)
                {

                }
            }
        }
    }

    private bool WithinBounds(int x, int y, float boundX, float boundY)
    {
        return !(x < 0 || y < 0 || x >= boundX || y >= boundY);
    }
}
