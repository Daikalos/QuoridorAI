public static class AI_Data
{
    public const uint predictionFreq = 4;       // The less, more often to try to prevent opponent from placing walls against you
    public const uint wallPlacementPrio = 3;    // The higher, prioritize more valuable wall placements
    public const uint prioDefense = 4;          // Steps from goal; The higher, sacrifice valuable wall placements and focus more on blocking opponent
    public const float edgeWeightFactor = 0.05f; // The higher, the more try to steer away from walls
}
