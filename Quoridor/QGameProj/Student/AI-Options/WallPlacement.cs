using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class WallPlacement
{
    private Graph graph;

    private Spelare player;
    private Spelare opponent;

    public WallPlacement(SpelBräde board)
    {
        graph = new Graph(board);
        graph.GenerateGraph();

        player = board.spelare[0];
        opponent = board.spelare[1];
    }

    public void PlaceWall(ref Drag move, SpelBräde board, List<Vertex> plyPath, List<Vertex> oppPath)
    {
        // Find which path is the longest and use it to determine best placement
        Dictionary<List<Point>, Tuple<Point, Typ>> bestPath = 
            new Dictionary<List<Point>, Tuple<Point, Typ>>();

        int i = 0; // Only allow this many iterations
        while (i < (graph.boardHeight * graph.boardWidth))
        {
            // Iterate through every vertex on path
            for (int j = 0; j < oppPath.Count - 1; ++j)
            {
                Vertex vertexFrom = oppPath[j];
                Vertex vertexTo = oppPath[j + 1];

                Point offset = vertexFrom.Position - vertexTo.Position;

                // Which wall to place depending on if opponent is going horizontally or vertically
                bool[,] cpyWallsVert = board.vertikalaLångaVäggar;
                bool[,] cpyWallsHori = board.horisontellaLångaVäggar;

                int x = 0;
                int y = 0;

                bool placeVertical = (Math.Abs(offset.X) > 0);

                if (placeVertical)
                    PlaceVerticalWall(board, vertexFrom, offset, cpyWallsVert, out x, out y);

                if (!placeVertical)
                    PlaceHorizontalWall(board, vertexFrom, offset, cpyWallsHori, out x, out y);

                graph = new Graph(board); // Generate new graph to test current placements
                graph.GenerateGraph(cpyWallsVert, cpyWallsHori);

                List<Vertex> oppDestVertices = new List<Vertex>();
                for (int k = 0; k < graph.boardWidth; k++)
                    oppDestVertices.Add(graph.AtPos(k, 0));

                List<Vertex> newPath = A_Star.PathTo(graph,
                    graph.AtPos(opponent.position),
                    oppDestVertices.ToArray());

                // If this new path is longer than opponents shortest path, then (x, y) is a suitable position to place a wall at
                if (newPath.Count > oppPath.Count && newPath.Count != 0)
                {
                    List<Point> pathPos = new List<Point>();
                    newPath.ForEach(v => pathPos.Add(v.Position));

                    bestPath.Add(pathPos, 
                        new Tuple<Point, Typ>(new Point(x, y), 
                        (placeVertical) ? Typ.Vertikal : Typ.Horisontell));
                }
            }
            ++i;
        }

        // When all suitable placements has been evaluated, return best placement for wall
        if (bestPath.Count > 0)
            move = BestPlacement(bestPath);
    }

    private void PlaceVerticalWall(SpelBräde board, Vertex vertexFrom, Point offset, bool[,] cpyWallsVert, out int x, out int y)
    {
        x = vertexFrom.Position.X;
        y = vertexFrom.Position.Y - 1;

        if (offset.X > 0 && x > 0) x--;
        if (y == -1) y = 0;

        for (int k = -1; k <= 1; ++k)
        {
            if (Graph.WithinBounds(x, y + k, graph.wallLengthX, graph.wallLengthY) && board.vertikalaLångaVäggar[x, y + k])
            {
                if (Graph.WithinBounds(x, y - k, graph.wallLengthX, graph.wallLengthY))
                    y = y - k;
            }
        }

        int validX = 0;
        while (Graph.WithinBounds(x + validX, y, graph.wallLengthX, graph.wallLengthY) && board.horisontellaLångaVäggar[x + validX, y]) // If there is wall already placed here, place next to it
            validX += (offset.X < 0) ? 1 : -1;

        x += validX;

        for (int k = -1; k <= 1; k += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x, y + k, graph.wallLengthX, graph.wallLengthY) && board.vertikalaLångaVäggar[x, y + k])
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !board.vertikalaLångaVäggar[x, y]) return;
        }

        cpyWallsVert[x, y] = true;
    }

    private void PlaceHorizontalWall(SpelBräde board, Vertex vertexFrom, Point offset, bool[,] cpyWallsHori, out int x, out int y)
    {
        x = vertexFrom.Position.X - 1;
        y = vertexFrom.Position.Y;

        if (offset.Y > 0 && y > 0) y--;
        if (x == -1) x = 0;

        for (int k = -1; k <= 1; ++k)
        {
            if (Graph.WithinBounds(x + k, y, graph.wallLengthX, graph.wallLengthY) && board.horisontellaLångaVäggar[x + k, y])
            {
                if (Graph.WithinBounds(x - k, y, graph.wallLengthX, graph.wallLengthY))
                    x = x - k;
            }
        }

        int validY = 0;
        while (Graph.WithinBounds(x, y + validY, graph.wallLengthX, graph.wallLengthY) && board.vertikalaLångaVäggar[x, y + validY]) // If there is wall already placed here, place next to it
            validY += (offset.Y < 0) ? 1 : -1;

        y += validY;

        for (int k = -1; k <= 1; k += 2) // Return if not valid wall placement
        {
            if (Graph.WithinBounds(x + k, y, graph.wallLengthX, graph.wallLengthY) && board.horisontellaLångaVäggar[x + k, y])
                if (Graph.WithinBounds(x, y, graph.wallLengthX, graph.wallLengthY) && !board.horisontellaLångaVäggar[x, y]) return;
        }

        cpyWallsHori[x, y] = true;
    }

    private Drag BestPlacement(Dictionary<List<Point>, Tuple<Point, Typ>> bestPath)
    {
        Drag move = new Drag();

        int largest = 0;
        foreach (List<Point> path in bestPath.Keys)
        {
            if (path.Count > largest)
            {
                move.typ = bestPath[path].Item2;
                move.point = bestPath[path].Item1;

                largest = path.Count;
            }
        }

        return move;
    }
}
