using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

class Graph
{
    private SpelBräde board;

    private List<Vertex> Vertices;
    private List<Edge> Edges;

    public int boardWidth { get; private set; }
    public int boardHeight { get; private set; }

    public int wallLengthX { get; private set; }
    public int wallLengthY { get; private set; }

    public Graph(SpelBräde board)
    {
        this.board = board;

        Edges = new List<Edge>();
        Vertices = new List<Vertex>();

        boardWidth = board.horisontellaLångaVäggar.GetLength(0) + 1;
        boardHeight = board.horisontellaLångaVäggar.GetLength(1) + 1;

        wallLengthX = board.horisontellaLångaVäggar.GetLength(0);
        wallLengthY = board.horisontellaLångaVäggar.GetLength(1);
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
        return (float)Math.Sqrt(
            Math.Pow(from.Position.X - to.Position.X, 2) +
            Math.Pow(from.Position.Y - to.Position.Y, 2));
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

                    Vertex neighbour = edge.To;
                    foreach (Edge nEdge in neighbour.Edges)
                    {
                        if (nEdge.To == edge.From)
                        {
                            edge.Weight = 1 + ((4.0f - neighbour.EdgeCount) * AI_Data.edgeWeightFactor);
                            break;
                        }
                    }
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
                Vertices.Add(new Vertex(new Point(x, y)));
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

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour));
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

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour));
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate custom graph
    /// </summary>
    public void GenerateGraph(bool[,] verticalWalls, bool[,] horizontalWalls)
    {
        AddVertices();
        AddEdges(verticalWalls, horizontalWalls);
    }
    private void AddEdges(bool[,] verticalWalls, bool[,] horizontalWalls)
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
                                addEdge = !verticalWalls[xPos, yPos + k];

                            if (!addEdge) break;
                        }

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour));
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
                                addEdge = !horizontalWalls[xPos + k, yPos];

                            if (!addEdge) break;
                        }

                        if (addEdge) Edges.Add(new Edge(vertex, neighbour));
                    }
                }
            }
        }
    }
}
