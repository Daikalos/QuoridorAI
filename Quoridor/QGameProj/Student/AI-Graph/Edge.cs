class Edge
{
    public Vertex To { get; private set; }
    public float Weight { get; set; }

    public Edge(Vertex from, Vertex to, float weight = 1)
    {
        To = to;
        Weight = weight;

        from.AddEdge(this);
    }
}
