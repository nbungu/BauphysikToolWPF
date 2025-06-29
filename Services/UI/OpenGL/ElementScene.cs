using BauphysikToolWPF.Models.Domain;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace BauphysikToolWPF.Services.UI.OpenGL
{
    /// <summary>
    /// Manages a collection of layers and handles their rendering lifecycle.
    /// Automatically rebuilds the geometry buffer when layers change.
    /// </summary>
    public class ElementScene : IDisposable
    {
        private readonly List<Layer> _layers = new();
        private readonly LayerRenderer _renderer;
        private bool _needsRebuild = true;
        private bool _disposed;

        /// <summary>
        /// Gets the renderer used by this scene.
        /// </summary>
        public LayerRenderer Renderer => _renderer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementScene"/> class with the specified renderer.
        /// </summary>
        /// <param name="renderer">The layer renderer used to draw the scene.</param>
        public ElementScene(LayerRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        /// <summary>
        /// Adds a single layer to the scene and marks the geometry for rebuilding.
        /// </summary>
        /// <param name="layer">The layer to add.</param>
        public void AddLayer(Layer layer)
        {
            _layers.Add(layer);
            _layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            _needsRebuild = true;
        }

        /// <summary>
        /// Adds multiple layers to the scene and marks the geometry for rebuilding.
        /// </summary>
        /// <param name="layers">The layers to add.</param>
        public void AddLayers(IEnumerable<Layer> layers)
        {
            _layers.AddRange(layers);
            _layers.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
            _needsRebuild = true;
        }

        /// <summary>
        /// Removes a layer from the scene and marks the geometry for rebuilding.
        /// </summary>
        /// <param name="layer">The layer to remove.</param>
        public void RemoveLayer(Layer layer)
        {
            _layers.Remove(layer);
            _needsRebuild = true;
        }

        /// <summary>
        /// Renders the scene using the given projection matrix.
        /// Rebuilds the geometry if layers have changed.
        /// </summary>
        /// <param name="projection">The projection matrix used for rendering.</param>
        public void Render(Matrix4 projection)
        {
            if (_needsRebuild)
            {
                _renderer.RebuildGeometry(_layers);
                _needsRebuild = false;
            }

            _renderer.Render(projection);
        }

        /// <summary>
        /// Disposes resources used by the scene and its renderer.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _renderer?.Dispose();
            _disposed = true;
        }
    }
}
