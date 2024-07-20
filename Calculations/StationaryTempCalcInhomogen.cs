using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BauphysikToolWPF.SessionData;
using System.Collections.Generic;
using System.Linq;
using Geometry;

namespace BauphysikToolWPF.Calculations
{
    public class StationaryTempCalcInhomogen
    {
        protected double _rsi = UserSaved.Rsi;
        protected double _rse = UserSaved.Rse;

        private Dictionary<string, object> _layerMapping = new Dictionary<string, object>();
        private Dictionary<int, List<string>> _areaToLayerPathCombinations = new Dictionary<int, List<string>>();
        private Dictionary<string, int[]> _layerToAreasMapping = new Dictionary<string, int[]>();
        private Dictionary<int, Rectangle> _areaMapping = new Dictionary<int, Rectangle>();

        public Element Element { get; } = new Element();
        public bool IsValid { get; }

        public StationaryTempCalcInhomogen() { }

        public StationaryTempCalcInhomogen(Element element)
        {
            if (element.Layers.Count == 0 || element is null) return;

            Element = element;
            Element.SortLayers();

            GetLayerMapping();
            GetLayerPathCombinations(0, new List<string>());
            GetLayerToRectanglesMapping();
            GetAreaMapping();


            IsValid = true;
        }


        private void GetLayerPathCombinations(int i, List<string> currentCombination)
        {
            if (i == Element.Layers.Count)
            {
                _areaToLayerPathCombinations.Add(_areaToLayerPathCombinations.Count + 1, currentCombination);
                return;
            }

            var layer = Element.Layers[i];
                
            if (layer.HasSubConstructions)
            {
                var newCombinationPath = new List<string>(currentCombination);
                newCombinationPath.Add($"{layer.LayerPosition}b");
                GetLayerPathCombinations(i + 1, newCombinationPath);
            }

            currentCombination.Add(layer.LayerPosition.ToString());
            GetLayerPathCombinations(i + 1, currentCombination);
        }

        private void GetLayerMapping()
        {
            foreach (var layer in Element.Layers)
            {
                _layerMapping.Add($"{layer.LayerPosition}", layer);
                if (layer.HasSubConstructions)
                {
                    _layerMapping.Add($"{layer.LayerPosition}b", layer.SubConstruction);
                }
            }
        }

        private void GetAreaMapping()
        {
            foreach (var kvp in _areaToLayerPathCombinations)
            {
                _areaMapping.Add(kvp.Key, new Rectangle());
            }
        }

        private void GetLayerToRectanglesMapping()
        {
            foreach (var layerString in _layerMapping.Keys)
            {
                int[] correspondingRectangles = _areaToLayerPathCombinations.Where(kvp => kvp.Value.Contains(layerString)).Select(kvp => kvp.Key).ToArray();
                _layerToAreasMapping.Add(layerString, correspondingRectangles);
            }
        }
    }
}
