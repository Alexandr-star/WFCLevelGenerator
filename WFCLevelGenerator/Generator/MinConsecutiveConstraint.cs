namespace Generator
{
    public class MinConsecutiveConstraint 
    {
        public int MinCount { get; set; }
        public string Tile { get; set; }
        
        public int IndexTile { get; set; }
        public bool AsixX { get; set; }
        public bool AsixY { get; set; }
        
        public MinConsecutiveConstraint(int minCount, string tile, bool asixX, bool asixY, int indexTile)
        {
            MinCount = minCount;
            Tile = tile;
            IndexTile = indexTile;
            AsixX = asixX;
            AsixY = asixY;
        }
    }
}
