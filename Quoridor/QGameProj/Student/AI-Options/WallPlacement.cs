using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class WallPlacement
{
    private Graph graph;
    private SpelBräde board;

    private Spelare player;
    private Spelare opponent;

    // Find which path is the longest and use it to determine best placement
    private List<Tuple<Drag, int>> bestPath;
    private List<Vertex> oppPath;

    private bool[,] cpyWallsVert;
    private bool[,] cpyWallsHori;

    private bool prioDef;

    public WallPlacement(SpelBräde board, List<Vertex> oppPath, bool prioDef)
    {
        this.board = board;
        this.oppPath = oppPath;
        this.prioDef = prioDef;

        graph = new Graph(board);
        graph.GenerateGraph();

        player = board.spelare[0];
        opponent = board.spelare[1];

        bestPath = new List<Tuple<Drag, int>>();
    }

    public void PlaceWall(ref Drag move)
    {
        // Iterate through every vertex on path
        for (int i = 0; i < oppPath.Count - 1; ++i)
        {
            TestWallPlacement(i, 0, 0);

            for (int j = -1; j <= 1; j += 2)
                TestWallPlacement(i, j, 0);

            for (int k = -1; k <= 1; k += 2)
                TestWallPlacement(i, 0, k);
        }

        // When all suitable placements has been evaluated, return best placement for wall
        if (bestPath.Count > 0)
            move = BestPlacement();
    }

    private void TestWallPlacement(int i, int j, int k)
    {
        Vertex vertexFrom = oppPath[i];
        Vertex vertexTo = oppPath[i + 1];

        Point offset = vertexFrom.Position - vertexTo.Position;
        Point placeAt = new Point(
            vertexFrom.Position.X + j,
            vertexFrom.Position.Y + k);

        bool placeVertical = (Math.Abs(offset.X) > 0);

        // Which wall to place depending on if opponent is going horizontally or vertically
        cpyWallsVert = (bool[,])board.vertikalaLångaVäggar.Clone();
        cpyWallsHori = (bool[,])board.horisontellaLångaVäggar.Clone();

        int x = -1;
        int y = -1;

        if (Graph.WithinBounds(placeAt, graph.boardWidth, graph.boardHeight))
        {
            if (placeVertical) PlaceVerticalWall(placeAt, offset, out x, out y);
            else               PlaceHorizontalWall(placeAt, offset, out x, out y);
        }

        if (x == -1 || y == -1)
            return;

        if (board.vertikalaLångaVäggar[x, y] || board.horisontellaLångaVäggar[x, y])
            return;

        graph = new Graph(board); // Generate new graph to test current placements
        graph.GenerateGraph(cpyWallsVert, cpyWallsHori);

        List<Vertex> oppDestVertices = new List<Vertex>();
        for (int l = 0; l < graph.boardWidth; l++)
            oppDestVertices.Add(graph.AtPos(l, 0));

        List<Vertex> oppNewPath = A_Star.PathTo(graph,
            graph.AtPos(opponent.position),
            oppDestVertices.ToArray());

        List<Vertex> plyDestVertices = new List<Vertex>();
        for (int l = 0; l < graph.boardWidth; l++)
            plyDestVertices.Add(graph.AtPos(l, graph.boardHeight - 1));

        List<Vertex> plyNewPath = A_Star.PathTo(graph,
            graph.AtPos(player.position),
            plyDestVertices.ToArray());

        if (oppNewPath.Count == 0 || plyNewPath.Count == 0)
            return;

        // If this new path is longer for opponent than player, then (x, y) is a suitable position to place a wall at
        if (oppNewPath.Count > plyNewPath.Count || (prioDef && oppNewPath.Count > oppPath.Count))
        {
            bestPath.Add(new Tuple<Drag, int>(new Drag
            {
                point = new Point(x, y),
                typ = (placeVertical) ? Typ.Vertikal : Typ.Horisontell
            }, oppNewPath.Count));
        }
    }

    private void PlaceVerticalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && cpyWallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y - i, graph.wallLengthX, graph.wallLengthY))
                {
                    y = y - i;
                }
            }
        }

        int xValid = 0;
        while (x > 0 && Graph.WithinBounds(x + xValid, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsHori[x + xValid, y])
            xValid += -offset.X;

        x += xValid;

        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && cpyWallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !cpyWallsVert[x, y])
                {
                    return;
                }
            }
        }

        cpyWallsVert[x, y] = true;
    }
    private void PlaceHorizontalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x - i, y, graph.wallLengthX, graph.wallLengthY))
                {
                    x = x - i;
                }
            }
        }

        int yValid = 0;
        while (y > 0 && Graph.WithinBounds(x, y + yValid, graph.wallLengthX, graph.wallLengthY) && cpyWallsVert[x, y + yValid])
            yValid += -offset.Y;

        y += yValid;

        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !cpyWallsHori[x, y])
                {
                    return;
                }
            }
        }

        cpyWallsHori[x, y] = true;
    }

    private Drag BestPlacement()
    {
        Drag move = new Drag();

        int largest = 0;
        foreach (Tuple<Drag, int> pathMove in bestPath)
        {
            if (pathMove.Item2 > largest)
            {
                move = pathMove.Item1;
                largest = pathMove.Item2;
            }
        }

        return move;
    }
}
