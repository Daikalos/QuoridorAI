using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class Graph
{
    private readonly SpelBräde board;
    private readonly Spelare opponent;

    private Vertex[] Vertices;

    public int boardWidth { get; private set; }
    public int boardHeight { get; private set; }

    public int wallWidth { get; private set; }
    public int wallHeight { get; private set; }

    public Graph(SpelBräde board)
    {
        this.board = board;
        opponent = board.spelare[1];

        boardWidth = board.horisontellaLångaVäggar.GetLength(0) + 1;
        boardHeight = board.horisontellaLångaVäggar.GetLength(1) + 1;

        wallWidth = board.horisontellaLångaVäggar.GetLength(0);
        wallHeight = board.horisontellaLångaVäggar.GetLength(1);

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

    public bool WithinWalls(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= wallWidth || y >= wallHeight);
    }
    public bool WithinWalls(Point pos)
    {
        return !(pos.X < 0 || pos.Y < 0 || pos.X >= wallWidth || pos.Y >= wallHeight);
    }

    public bool WithinBoard(int x, int y)
    {
        return !(x < 0 || y < 0 || x >= boardWidth || y >= boardHeight);
    }
    public bool WithinBoard(Point pos)
    {
        return !(pos.X < 0 || pos.Y < 0 || pos.X >= boardWidth || pos.Y >= boardHeight);
    }

    public static float Distance(Vertex from, Vertex to)
    {
        return Math.Abs(from.Position.X - to.Position.X) +
               Math.Abs(from.Position.Y - to.Position.Y); // Manhattan Distance
    }

    public Vertex[] PlayerGoal()
    {
        Vertex[] goalVertices = new Vertex[boardWidth];
        for (int x = 0; x < boardWidth; x++)
            goalVertices[x] = AtPos(x, boardHeight - 1);

        return goalVertices;
    }
    public Vertex[] OpponentGoal()
    {
        Vertex[] goalVertices = new Vertex[boardWidth];
        for (int x = 0; x < boardWidth; ++x)
            goalVertices[x] = AtPos(x, 0);

        return goalVertices;
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
                    if (WithinBoard((x + i), y))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x + i, y);

                        int xPos = x;
                        int yPos = y - 1;

                        if (i < 0 && x > 0) --xPos;

                        bool addEdge = true; // Don't add edge if wall between vertices
                        for (int k = 0; k < 2; ++k)
                        {
                            if (WithinWalls(xPos, yPos + k))
                                addEdge = !board.vertikalaLångaVäggar[xPos, yPos + k];

                            if (!addEdge) break;
                        }

                        if (addEdge) new Edge(vertex, neighbour);
                    }
                }
                for (int j = -1; j <= 1; j += 2) //Top and Bottom
                {
                    if (WithinBoard(x, (y + j)))
                    {
                        Vertex vertex = AtPos(x, y);
                        Vertex neighbour = AtPos(x, y + j);

                        int xPos = x - 1;
                        int yPos = y;

                        if (j < 0 && y > 0) --yPos;

                        bool addEdge = true; // Don't add edge if wall between vertices
                        for (int k = 0; k < 2; ++k)
                        {
                            if (WithinWalls(xPos + k, yPos))
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
                    if (WithinBoard(x + i, y + j))
                        vertices.Add(AtPos(x + i, y + j));
                }
                else
                {
                    if (WithinBoard(x + j, y + i))
                        vertices.Add(AtPos(x + j, y + i));
                }
            }
        }

        for (int i = 0; i < vertices.Count; ++i)
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
                    if (WithinBoard(x + i, y + j))
                        vertices.Add(AtPos(x + i, y + j));
                }
                else
                {
                    if (WithinBoard(x + j, y + i))
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
