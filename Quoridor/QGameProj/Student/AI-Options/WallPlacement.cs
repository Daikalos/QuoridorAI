using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class WallPlacement
{
    private readonly Graph graph;

    private readonly Spelare player;
    private readonly Spelare opponent;

    private readonly bool[,] wallsVert;
    private readonly bool[,] wallsHori;

    private readonly bool prioDef;

    private List<Vertex> oppPath;
    private List<Vertex> plyPath;

    // Find which path is the longest and use it to determine best placement
    private List<Tuple<Drag, int>> bestMoves;

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
            for (int j = -2; j <= 2; j++)
            {
                for (int k = -2; k <= 2; k++)
                {
                    TestWallPlacement(i, j, k);
                }
            }
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

        if (!graph.WithinBoard(placeAt))
            return;

        // Which wall to place depending on if opponent is going horizontally or vertically
        bool placeVertical = (Math.Abs(offset.X) > 0);

        int x, y;

        if (placeVertical)
        {
            if (!PlaceVerticalWall(placeAt, out x, out y))
                return;
        }
        else
        {
            if (!PlaceHorizontalWall(placeAt, out x, out y))
                return;
        }

        if (wallsVert[x, y] || wallsHori[x, y])
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

    private bool PlaceVerticalWall(Point placeAt, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        if (!VerticalWallViable(x, y))
            return false;

        return true;
    }
    private bool PlaceHorizontalWall(Point placeAt, out int x, out int y)
    {
        x = placeAt.X;
        y = placeAt.Y;

        if (x > 0) x--;
        if (y > 0) y--;

        if (!HorizontalWallViable(x, y))
            return false;

        return true;
    }

    private bool VerticalWallViable(int x, int y)
    {
        for (int i = -1; i <= 1; i += 2) // Return if not valid wall placement
        {
            if (graph.WithinWalls(x, y + i) && wallsVert[x, y + i])
            {
                if (graph.WithinWalls(x, y) && !wallsVert[x, y])
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
            if (graph.WithinWalls(x + i, y) && wallsHori[x + i, y])
            {
                if (graph.WithinWalls(x, y) && !wallsHori[x, y])
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
