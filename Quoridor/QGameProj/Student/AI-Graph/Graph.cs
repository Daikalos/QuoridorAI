using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class Graph
{
    private SpelBräde board;
    private Spelare opponent;

    private Vertex[] Vertices;

    public int boardWidth { get; private set; }
    public int boardHeight { get; private set; }

    public int wallLengthX { get; private set; }
    public int wallLengthY { get; private set; }

    public Graph(SpelBräde board)
    {
        this.board = board;
        opponent = board.spelare[1];

        boardWidth = board.horisontellaLångaVäggar.GetLength(0) + 1;
        boardHeight = board.horisontellaLångaVäggar.GetLength(1) + 1;

        wallLengthX = board.horisontellaLångaVäggar.GetLength(0);
        wallLengthY = board.horisontellaLångaVäggar.GetLength(1);

        Vertices = new Vertex[boardWidth * boardHeight];
    }

    public Vertex AtPos(int x, int y)
    {
        return Vertices[x + y * boardWidth];
    }
    public Vertex AtPos(Point pos)
    {
        return Vertices[pos.X + pos.Y * boardWidth];
    }

    public static bool WithinBounds(int x, int y, float boundsWidth, float boundsHeight)
    {
        return !(x < 0 || y < 0 || x >= boundsWidth || y >= boundsHeight);
    }
    public static bool WithinBounds(Point pos, float boundsWidth, float boundsHeight)
    {
        return !(pos.X < 0 || pos.Y < 0 || pos.X >= boundsWidth || pos.Y >= boundsHeight);
    }

    public static float Distance(Vertex from, Vertex to)
    {
        return Math.Abs(from.Position.X - to.Position.X) +
               Math.Abs(from.Position.Y - to.Position.Y); // Manhattan Distance
    }

    public Vertex[] PlayerGoal()
    {
        List<Vertex> goalVertices = new List<Vertex>();
        for (int x = 0; x < boardWidth; x++)
            goalVertices.Add(AtPos(x, boardHeight - 1));

        return goalVertices.ToArray();
    }
    public Vertex[] OpponentGoal()
    {
        List<Vertex> goalVertices = new List<Vertex>();
        for (int x = 0; x < boardWidth; ++x)
            goalVertices.Add(AtPos(x, 0));

        return goalVertices.ToArray();
    }

    public void InitializeVertices()
    {
        foreach (Vertex vertex in Vertices)
        {
            vertex.IsVisited = false;
            vertex.G = float.PositiveInfinity;
            vertex.H = float.PositiveInfinity;
        }
    }
    public void SetEdgeWeight()
    {
        // Set the weight of each unique edge depending on number of paths available
        for (int y = 0; y < boardHeight; ++y)
        {
            for (int x = 0; x < boardWidth; ++x)
            {
                Vertex vertex = AtPos(x, y);
                foreach (Edge edge in vertex.Edges)
                {
                    if (vertex.EdgeCount == 0)
                        break;

                    edge.Weight = 1 + ((4.0f - edge.To.EdgeCount) * (opponent.antalVäggar / 10.0f) * AI_Data.edgeWeightFactor);
                }
            }
        }
    }

    public void GenerateGraph()
    {
        AddVertices();
        AddEdges();
    }
    private void AddVertices()
    {
        for (int y = 0; y < boardHeight; y++) // Add all vertices
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Vertices[x + y * boardWidth] = new Vertex(new Point(x, y));
            }
        }
    }
    private void AddEdges()
    {
        for (int y = 0; y < boardHeight; ++y) // Add all edges
        {
            for (int x = 0; x < boardWidth; ++x)
            {
                for (int i = -1; i <= 1; i += 2) // Left and Right
                {
                    if (WithinBounds((x + i), y, boardWidth, boardHeight))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x + i, y);

                        int xPos = x;
                        int yPos = y - 1;

                        if (i < 0 && x > 0) --xPos;

                        bool addEdge = true; // Don't add edge if wall between vertices
                        for (int k = 0; k < 2; ++k)
                        {
                            if (WithinBounds(xPos, yPos + k, wallLengthX, wallLengthY))
                                addEdge = !board.vertikalaLångaVäggar[xPos, yPos + k];

                            if (!addEdge) break;
                        }

                        if (addEdge) new Edge(vertex, neighbour);
                    }
                }
                for (int j = -1; j <= 1; j += 2) //Top and Bottom
                {
                    if (WithinBounds(x, (y + j), boardWidth, boardHeight))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x, y + j);

                        int xPos = x - 1;
                        int yPos = y;

                        if (j < 0 && y > 0) --yPos;

                        bool addEdge = true; // Don't add edge if wall between vertices
                        for (int k = 0; k < 2; ++k)
                        {
                            if (WithinBounds(xPos + k, yPos, wallLengthX, wallLengthY))
                                addEdge = !board.horisontellaLångaVäggar[xPos + k, yPos];

                            if (!addEdge) break;
                        }

                        if (addEdge) new Edge(vertex, neighbour);
                    }
                }
            }
        }
    }

    public void AddWall(bool wallType, int x, int y)
    {
        // wallType = true;  Vertical wall
        // wallType = false; Horizontal wall

        List<Vertex> vertices = new List<Vertex>();
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                if (wallType)
                {
                    if (WithinBounds(x + i, y + j, boardWidth, boardHeight))
                        vertices.Add(AtPos(x + i, y + j));
                }
                else
                {
                    if (WithinBounds(x + j, y + i, boardWidth, boardHeight))
                        vertices.Add(AtPos(x + j, y + i));
                }
            }
        }

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (vertices[i].Neighbours.Contains(vertices[(i + 2) % 4]))
            {
                foreach (Edge edge in vertices[i].Edges)
                {
                    if (edge.To == vertices[(i + 2) % 4])
                    {
                        vertices[i].RemoveEdge(edge);
                        break;
                    }
                }
            }
        }
    }
    public void RemoveWall(bool wallType, int x, int y)
    {
        // wallType = true;  Vertical wall
        // wallType = false; Horizontal wall

        List<Vertex> vertices = new List<Vertex>();
        for (int i = 0; i < 2; ++i)
        {
            for (int j = 0; j < 2; ++j)
            {
                if (wallType)
                {
                    if (WithinBounds(x + i, y + j, boardWidth, boardHeight))
                        vertices.Add(AtPos(x + i, y + j));
                }
                else
                {
                    if (WithinBounds(x + j, y + i, boardWidth, boardHeight))
                        vertices.Add(AtPos(x + j, y + i));
                }
            }
        }

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (!vertices[i].Neighbours.Contains(vertices[(i + 2) % 4]))
                new Edge(vertices[i], vertices[(i + 2) % 4]);
        }
    }
}
