class Edge
{
    public Vertex From { get; private set; }
    public Vertex To { get; private set; }

    public float Weight { get; set; }

    public Edge(Vertex from, Vertex to, float weight = 1)
    {
        From = from;
        To = to;
        Weight = weight;

        from.AddEdge(this);
    }
}
