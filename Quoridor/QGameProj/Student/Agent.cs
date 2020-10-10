using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

class Agent : BaseAgent 
{
    [STAThread]
    static void Main() 
    {
        Program.Start(new Agent());
    }

    public Agent() {}

    public override Drag SökNästaDrag(SpelBräde bräde) 
    {
        return !(bräde.avanceradeRegler) ? Normal(bräde) : Advanced(bräde);
    }

    private Drag Normal(SpelBräde bräde)
    {
        throw new System.NotImplementedException();
    }

    private Drag Advanced(SpelBräde bräde)
    {
        Spelare player = bräde.spelare[0];
        Spelare opponent = bräde.spelare[1];

        Point playerPos = player.position;
        Point opponentPos = opponent.position;

        Drag move = new Drag();

        Graph graph = new Graph(bräde); // Generate a graph over map
        graph.GenerateGraph();

        SetEdgeWeight(graph, opponent);

        List<Vertex> destVertices = new List<Vertex>(); // Add goal vertices to find path to
        for (int x = 0; x < graph.boardWidth; x++) 
            destVertices.Add(graph.AtPos(x, graph.boardHeight - 1));

        // Use A* to find shortest path
        List<Vertex> path = A_Star.PathTo(
            graph.AtPos(playerPos),
            destVertices.ToArray());

        if (path.Count == 0) // No valid path found to goal
            throw new System.ArgumentOutOfRangeException();

        Point desiredPos = path[0].Position;

        // If desired position is on the opponents position
        if (desiredPos == opponentPos)
            desiredPos = MoveOverOpponent(graph, path, playerPos, opponentPos);

        move.typ = Typ.Flytta;
        move.point = desiredPos;

        if (player.antalVäggar > 0) // Check if to place a wall
            PlaceWall(ref move, bräde, graph, path);

        return move;
    }

    private void SetEdgeWeight(Graph graph, Spelare opponent)
    {
        // Set the weight of each unique edge depending on number of paths available
        for (int y = 0; y < graph.boardHeight; y++)
        {
            int x = (y % 2 == 1) ? 1 : 0;
            for (; x < graph.boardWidth; x += 2)
            {
                Vertex vertex = graph.AtPos(x, y);
                foreach (Edge edge in vertex.Edges)
                {
                    if (vertex.EdgeCount == 0)
                        break;

                    edge.Weight = 1 + ((4 / vertex.EdgeCount) * opponent.antalVäggar * 2.0f);
                }
            }
        }
    }

    private Point MoveOverOpponent(Graph graph, List<Vertex> path, Point playerPos, Point opponentPos)
    {
        Point offset = opponentPos - playerPos;
        Point desiredPos = playerPos + offset;

        if (offset.X == 1)  // Opponent is to the left
            desiredPos.X++;
        if (offset.X == -1) // Opponent is to the right
            desiredPos.X--;
        if (offset.Y == -1) // Opponent is up
            desiredPos.Y--;
        if (offset.Y == 1)  // Opponent is down
            desiredPos.Y++;

        Vertex plyVertex = graph.AtPos(playerPos);
        Vertex oppVertex = graph.AtPos(opponentPos);

        // If there is no path to desired position
        if (!Graph.WithinBounds(desiredPos, graph.boardWidth, graph.boardHeight) || !oppVertex.Neighbours.Contains(graph.AtPos(desiredPos)))
        {
            List<Vertex> desVertices = new List<Vertex>();

            // First see if we can move diagonally to one of opponents neighbours
            foreach (Vertex neighbour in oppVertex.Neighbours)
            {
                if (neighbour != plyVertex)
                {
                    desVertices.Add(neighbour);
                }
            }

            if (desVertices.Count > 0)
                return SuitableVertex(path, desVertices);

            // If not, set desired position to suitable neighbour next to player
            foreach (Vertex neighbour in plyVertex.Neighbours)
            {
                if (neighbour != oppVertex)
                {
                    desVertices.Add(neighbour);
                }
            }

            if (desVertices.Count > 0)
                return SuitableVertex(path, desVertices);
        }

        return desiredPos;
    }
    private Point SuitableVertex(List<Vertex> path, List<Vertex> desVertices)
    {
        // If the vertex we are trying to go to is already contained in path, go to it
        for (int i = 0; i < path.Count; i++)
        {
            for (int j = 0; j < desVertices.Count; j++) // At most 3
            {
                if (path[i] == desVertices[j])
                {
                    return path[i].Position;
                }
            }
        }

        // Else return with going to the vertex closest to goal
        return Min(path, desVertices.ToArray()).Position;
    }

    private Vertex Min(List<Vertex> path, params Vertex[] vertices)
    {
        // Return vertex of closest distance to goal

        Vertex closest = vertices[0];
        float smallest = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Graph.Distance(vertices[i], path[path.Count - 1]);
            if (distance < smallest)
            {
                closest = vertices[i];
                smallest = distance;
            }
        }

        return closest;
    }

    private void PlaceWall(ref Drag move, SpelBräde board, Graph graph, List<Vertex> plyPath)
    {
        Spelare player = board.spelare[0];
        Spelare opponent = board.spelare[1];

        List<Vertex> oppDestVertices = new List<Vertex>(); // Add opponents goal vertices to find path to
        for (int x = 0; x < graph.boardWidth; x++)
            oppDestVertices.Add(graph.AtPos(x, 0));

        List<Vertex> oppPath = A_Star.PathTo(
            graph.AtPos(opponent.position),
            oppDestVertices.ToArray());

        // If opponent is closer to goal than player
        if (oppPath.Count < (plyPath.Count + (player.antalVäggar / 3)))
        {
            int i = 0; // Only allow this many iterations
            List<Vertex> newPath = new List<Vertex>();
            while (oppPath.Count > newPath.Count && i < (graph.boardHeight * graph.boardWidth))
            {
                Graph testGraph = new Graph(board);

                for (int j = 0; j < oppPath.Count - 1; ++j)
                {
                    Vertex vertexFrom = oppPath[i];
                    Vertex vertexTo = oppPath[i + 1];

                    Point offset = vertexFrom.Position - vertexTo.Position;

                    // Which wall to place depending on if opponent is going horizontally or vertically
                    bool[,] copyWallsVert = board.vertikalaLångaVäggar;
                    bool[,] copyWallsHori = board.horisontellaLångaVäggar;

                    if (Math.Abs(offset.X) > 0) // Vertical walls
                    {
                        int x = vertexFrom.Position.X;
                        int y = vertexFrom.Position.Y - 1;

                        if (x > 0) x--;

                        copyWallsVert[x, y] = true;
                    }
                    else // Horizontal walls
                    {
                        int x = vertexFrom.Position.X - 1;
                        int y = vertexFrom.Position.Y;

                        if (y > 0) y--;

                        copyWallsHori[x, y] = true;
                    }

                    testGraph.GenerateGraph(copyWallsVert, copyWallsHori);
                }
                ++i;
            }
        }
    }

    public override Drag GörOmDrag(SpelBräde bräde, Drag drag)
    {
        System.Diagnostics.Debugger.Break();
        return SökNästaDrag(bräde);
    }
}
//enum Typ { Flytta, Horisontell, Vertikal }
//struct Drag {
//    public Typ typ;
//    public Point point;
//}