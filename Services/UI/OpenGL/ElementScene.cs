using BauphysikToolWPF.Models.Domain;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    public class ElementScene
    {
        private readonly List<Layer> _layers = new();
        private readonly LayerRenderer _renderer;

        public ElementScene(LayerRenderer renderer)
        {
            _renderer = renderer;
        }

        public void AddLayer(Layer layer)
        {
            _layers.Add(layer);
            _layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
        }

        public void Render(Matrix4 projection)
        {
            foreach (var layer in _layers)
            {
                //layer.UpdateBrushCache(); // TODO: Test
                _renderer.Render(layer, projection);
            }
        }
    }
}
