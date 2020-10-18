using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class WallPlacement
{
    private Graph graph;

    private Spelare player;
    private Spelare opponent;

    private List<Vertex> oppPath;
    private List<Vertex> plyPath;

    // Find which path is the longest and use it to determine best placement
    private List<Tuple<Drag, int>> bestMoves;

    private readonly bool[,] wallsVert;
    private readonly bool[,] wallsHori;

    private bool prioDef;

    public WallPlacement(SpelBräde board, Graph graph, bool prioDef)
    {
        this.graph = graph;
        this.prioDef = prioDef;

        player = board.spelare[0];
        opponent = board.spelare[1];

        oppPath = A_Star.PathTo(graph, false,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        plyPath = A_Star.PathTo(graph, false,
            graph.AtPos(player.position),
            graph.PlayerGoal());

        wallsVert = board.vertikalaLångaVäggar;
        wallsHori = board.horisontellaLångaVäggar;

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

            for (int l = -2; l <= 2; ++l)
                if (l != 0) TestWallPlacement(i, -l, l);
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

        int x = -1;
        int y = -1;

        if (Graph.WithinBounds(placeAt, graph.boardWidth, graph.boardHeight))
        {
            if (placeVertical)
            {
                if (!PlaceVerticalWall(placeAt, offset, out x, out y))
                    return;
            }
            else
            {
                if (!PlaceHorizontalWall(placeAt, offset, out x, out y))
                    return;
            }
        }

        if (x == -1 || y == -1 || wallsVert[x, y] || wallsHori[x, y])
            return;

        graph.AddWall(placeVertical, x, y);

        List<Vertex> oppNewPath = A_Star.PathTo(graph, false,
            graph.AtPos(opponent.position),
            graph.OpponentGoal());

        List<Vertex> plyNewPath = A_Star.PathTo(graph, false,
            graph.AtPos(player.position),
            graph.PlayerGoal());

        graph.RemoveWall(placeVertical, x, y);

        if (oppNewPath.Count == 0 || plyNewPath.Count == 0)
            return;

        int oppPathCount = Math.Max(0, oppNewPath.Count - oppPath.Count);
        int plyPathCount = Math.Max(0, plyNewPath.Count - plyPath.Count);

        // If this new path is much longer for opponent than player, then (x, y) is a suitable position to place a wall at
        if (((oppPathCount - plyPathCount) > AI_Data.wallPlacementPrio) || prioDef)
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

    private bool PlaceVerticalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y - i, graph.wallLengthX, graph.wallLengthY))
                {
                    y = y - i;
                }
            }
        }

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x, y])
        {
            if (Graph.WithinBounds(x - offset.X, y, graph.wallLengthX, graph.wallLengthY))
            {
                x += -offset.X;
            }
        }

        if (!VerticalWallViable(x, y))
            return false;


        return true;
    }
    private bool PlaceHorizontalWall(Point placeAt, Point offset, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        for (int i = -1; i <= 1; i += 2)
        {
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x - i, y, graph.wallLengthX, graph.wallLengthY))
                {
                    x = x - i;
                }
            }
        }

        if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y])
        {
            if (Graph.WithinBounds(x, y - offset.Y, graph.wallLengthX, graph.wallLengthY))
            {
                y += -offset.Y;
            }
        }

        if (!HorizontalWallViable(x, y))
            return false;

        return true;
    }

    private bool VerticalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x, y + i, graph.wallLengthX, graph.wallLengthY) && wallsVert[x, y + i])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !wallsVert[x, y])
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
            if (Graph.WithinBounds(x + i, y, graph.wallLengthX, graph.wallLengthY) && wallsHori[x + i, y])
            {
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !wallsHori[x, y])
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
