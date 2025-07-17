namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public enum ShapeType
    {
        Layer = 0,
        SubConstructionLayer = 1,
        DimensionalChain = 2,
        Annotation = 3,
        // Extend as needed
    }

    public struct ShapeId
    {
        public ShapeType Type { get; }
        public int Index { get; }

        public ShapeId(ShapeType type, int index)
        {
            Type = type;
            Index = index;
        }

        public override string ToString() => $"{Type}[{Index}]";
    }
}
