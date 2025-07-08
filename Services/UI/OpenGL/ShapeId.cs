namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public enum ShapeType
    {
        Layer = 0,
        DimensionalChain = 1,
        Annotation = 2,
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
