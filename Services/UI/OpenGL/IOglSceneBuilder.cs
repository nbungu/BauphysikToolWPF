using BauphysikToolWPF.Models.UI;
using BT.Geometry;
using System.Collections.Generic;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public interface IOglSceneBuilder
    {
        public List<float> RectVertices { get; }
        public List<float> LineVertices { get; }
        public List<(int? TextureId, int Count)> RectBatches { get; }
        public List<(float LineWidth, int Count)> LineBatches { get; }
        public Rectangle SceneBounds { get; }
        public List<IDrawingGeometry> SceneShapes { get; }
        public bool IsTextSizeZoomable { get; set; }
        public void BuildScene();
        public void ConnectToController(OglController controller);
    }
}
