using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class WallPlacement
{
    private Graph graph;
    private SpelBräde board;

    private Spelare player;
    private Spelare opponent;

    // Find which path is the longest and use it to determine best placement
    private List<Tuple<Drag, int>> bestMoves;
    private List<Vertex> oppPath;
    private List<Vertex> plyPath;

    private bool[,] cpyWallsVert;
    private bool[,] cpyWallsHori;

    private bool prioDef;

    public WallPlacement(SpelBräde board, List<Vertex> oppPath, List<Vertex> plyPath, bool prioDef)
    {
        this.board = board;
        this.oppPath = oppPath;
        this.plyPath = plyPath;
        this.prioDef = prioDef;

        graph = new Graph(board);
        graph.GenerateGraph();

        player = board.spelare[0];
        opponent = board.spelare[1];

        bestMoves = new List<Tuple<Drag, int>>();
    }

    public void PlaceWall(ref Drag move)
    {
        // Iterate through every vertex on path
        for (int i = 0; i < oppPath.Count - 1; ++i)
        {
            TestWallPlacement(i, 0, 0);

            for (int j = -2; j <= 2; ++j)
                if (j != 0) TestWallPlacement(i, j, 0);

            for (int k = -2; k <= 2; ++k)
                if (k != 0) TestWallPlacement(i, 0, k);

            for (int l = -2; l <= 2; ++l)
                if (l != 0) TestWallPlacement(i, l, l);

            for (int m = -2; m <= 2; ++m)
                if (m != 0) TestWallPlacement(i, -m, m);
        }

        // When all suitable placements has been evaluated, return best placement for wall
        if (bestMoves.Count > 0)
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

        // Which wall to place depending on if opponent is going horizontally or vertically
        bool placeVertical = (Math.Abs(offset.X) > 0);

        cpyWallsVert = (bool[,])board.vertikalaLångaVäggar.Clone();
        cpyWallsHori = (bool[,])board.horisontellaLångaVäggar.Clone();

        int x = -1;
        int y = -1;

        if (Graph.WithinBounds(placeAt, graph.boardWidth, graph.boardHeight))
        {
            if (placeVertical) PlaceVerticalWall(placeAt, offset, out x, out y);
            else               PlaceHorizontalWall(placeAt, offset, out x, out y);
        }

        if (x == -1 || y == -1 || board.vertikalaLångaVäggar[x, y] || board.horisontellaLångaVäggar[x, y])
            return;

        if ((placeVertical && !cpyWallsVert[x, y]) || (!placeVertical && !cpyWallsHori[x, y]))
            return;

        graph = new Graph(board); // Generate new temporary graph to test current placements
        graph.GenerateGraph(cpyWallsVert, cpyWallsHori);

        List<Vertex> oppNewPath = A_Star.PathTo(graph,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        List<Vertex> plyNewPath = A_Star.PathTo(graph,
            graph.AtPos(player.position),
            graph.PlayerGoal());

        if (oppNewPath.Count == 0 || plyNewPath.Count == 0)
            return;

        int oppPathCount = oppNewPath.Count - oppPath.Count;
        int plyPathCount = plyNewPath.Count - plyPath.Count;

        // If this new path is much longer for opponent than player, then (x, y) is a suitable position to place a wall at
        if (((oppPathCount - AI_Data.wallPlacementPrio) > plyPathCount) || prioDef)
        {
            if (oppPathCount > 0)
            {
                bestMoves.Add(new Tuple<Drag, int>(new Drag
                {
                    point = new Point(x, y),
                    typ = (placeVertical) ? Typ.Vertikal : Typ.Horisontell
                }, oppPathCount));
            }
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

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsHori[x, y])
        {
            if (Graph.WithinBounds(x - offset.X, y, graph.wallLengthX, graph.wallLengthY))
            {
                x += -offset.X;
            }
        }

        if (!VerticalWallViable(x, y))
            return;

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

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsVert[x, y])
        {
            if (Graph.WithinBounds(x, y - offset.Y, graph.wallLengthX, graph.wallLengthY))
            {
                y += -offset.Y;
            }
        }

        if (!HorizontalWallViable(x, y))
            return;

        cpyWallsHori[x, y] = true;
    }

    private bool VerticalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && cpyWallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !cpyWallsVert[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }
    private bool HorizontalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && cpyWallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !cpyWallsHori[x, y])
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Drag BestPlacement()
    {
        Drag move = new Drag();

        int largest = 0;
        foreach (Tuple<Drag, int> bestMove in bestMoves)
        {
            if (bestMove.Item2 > largest)
            {
                move = bestMove.Item1;
                largest = bestMove.Item2;
            }
        }

        return move;
    }
}
