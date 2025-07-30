namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public enum ShapeType
    {
        None = -1,
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
        public static readonly ShapeId None = new ShapeId(ShapeType.None, -1);
        public override string ToString() => $"{Type}[{Index}]";
    }
}
