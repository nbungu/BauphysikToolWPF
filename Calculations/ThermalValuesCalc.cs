using BauphysikToolWPF.Models;
using BauphysikToolWPF.Models.Helper;
using BT.Geometry;
using BT.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BauphysikToolWPF.Calculations
{
    public class ThermalValuesCalc
    {
        private Rectangle _calculationAreaBounds = new Rectangle();
        private readonly Dictionary<string, object> _layerMapping = new Dictionary<string, object>();
        private readonly Dictionary<int, List<string>> _areaToLayerPathCombinations = new Dictionary<int, List<string>>();
        private readonly Dictionary<string, int[]> _layerToAreasMapping = new Dictionary<string, int[]>();
        private readonly Dictionary<int, Rectangle> _areaRectangleMapping = new Dictionary<int, Rectangle>();
        private readonly Dictionary<int, double> _areaSharesMapping = new Dictionary<int, double>();
        
        public Element Element { get; set; } = new Element();
        public double Ti { get; set; }
        public double Te { get; set; }
        public double Rsi { get; set; }
        public double Rse { get; set; }

        // Calculated Properties
        public List<Layer> RelevantLayers { get; private set; } = new List<Layer>();
        public double UValue { get; private set; }
        public double RTotal { get; private set; }
        public double RGes { get; private set; }
        public double QValue { get; private set; }
        public double ErrorEstimation { get; private set; }
        public bool ErrorEstimationOk { get; private set; }
        public bool IsValid { get; private set; }

        public ThermalValuesCalc() { }
        public ThermalValuesCalc(Element element, double rsi, double rse, double ti, double te)
        {
            if (element.Layers.Count == 0 || element is null) return;
            Element = element;
            Rsi = Math.Max(0, rsi);
            Rse = Math.Max(0, rse);
            Ti = ti;
            Te = te;

            if (Element.IsInhomogeneous) CalculateInhomogeneous();
            else CalculateHomogeneous();
        }

        public void CalculateHomogeneous()
        {
            RelevantLayers = Element.SortLayers().Layers.Where(l => l.IsEffective).ToList();
            try
            {
                double rGes = 0.0;
                foreach (var layer in RelevantLayers)
                {
                    rGes += layer.R_Value;
                }
                RGes = Math.Round(rGes, 3);
                RTotal = Math.Round(Rsi + rGes + Rse, 3);
                UValue = Math.Round(1 / (Rsi + rGes + Rse), 3);
                QValue = Math.Round(UValue * (Ti - Te), 3);
                ErrorEstimation = 0;
                ErrorEstimationOk = true;
                IsValid = true;
                Logger.LogInfo($"Successfully calculated homogeneous Element: {Element}");
            }
            catch (Exception ex)
            {
                IsValid = false;
                Logger.LogError($"Error calculating homogeneous Element: {Element}, {ex.Message}");
            }
        }

        public void CalculateInhomogeneous()
        {
            RelevantLayers = Element.SortLayers().Layers.Where(l => l.IsEffective).ToList();
            try
            {
                PrepareMappingsForInhomogeneous();

                // R_upper via cross section
                double r_tot_upper = 0.0;
                double r_ges_upper = 0.0;
                foreach (var kvp in _areaToLayerPathCombinations)
                {
                    var area = kvp.Key;
                    var layerPathCombo = kvp.Value;
                    double r_tot_area = Rsi + Rse;
                    double r_ges_area = 0;

                    foreach (var layerString in layerPathCombo)
                    {
                        // Check the Type: Use the is keyword to check the type or the as keyword to attempt a cast.
                        if (_layerMapping[layerString] is Layer layer)
                        {
                            r_tot_area += layer.R_Value;
                            r_ges_area += layer.R_Value;
                        }
                        else if (_layerMapping[layerString] is LayerSubConstruction layerSubConstr)
                        {
                            r_tot_area += layerSubConstr.R_Value;
                            r_ges_area += layerSubConstr.R_Value;
                        }
                        else throw new Exception("Typ konnte nicht aufgelöst werden");
                    }
                    r_tot_upper += _areaSharesMapping[area] / r_tot_area;
                    r_ges_upper += _areaSharesMapping[area] / r_ges_area;
                }
                r_tot_upper = Math.Pow(r_tot_upper, -1);
                r_ges_upper = Math.Pow(r_ges_upper, -1);

                // R_lower via vertical cut
                double r_tot_lower = Rsi + Rse;
                double r_ges_lower = 0.0;
                foreach (var layer in RelevantLayers)
                {
                    if (layer.HasSubConstructions)
                    {
                        string mainLayer = $"{layer.LayerPosition}";
                        string subLayer = $"{layer.LayerPosition}b";
                        var correspAreasMainLayer = _layerToAreasMapping[mainLayer];
                        var correspAreasSubLayer = _layerToAreasMapping[subLayer];

                        double combinedAreaMainLayer = 0.0;
                        foreach (var area in correspAreasMainLayer)
                        {
                            combinedAreaMainLayer += _areaSharesMapping[area];
                        }
                        double combinedAreaSubLayer = 0.0;

                        foreach (var area in correspAreasSubLayer)
                        {
                            combinedAreaSubLayer += _areaSharesMapping[area];
                        }
                        double r_area = (combinedAreaMainLayer / layer.R_Value) + (combinedAreaSubLayer / layer.SubConstruction.R_Value);
                        r_area = Math.Pow(r_area, -1);
                        r_tot_lower += r_area;
                        r_ges_lower += r_area;
                    }
                    else
                    {
                        r_tot_lower += layer.R_Value;
                        r_ges_lower += layer.R_Value;
                    }
                }
                // R_ges (without Rsi, Rse)
                double r_ges = (r_ges_upper + r_ges_lower) / 2;
                RGes = Math.Round(r_ges, 3);

                // R_tot (including Rsi, Rse)
                double r_tot = (r_tot_upper + r_tot_lower) / 2;
                RTotal = Math.Round(r_tot, 3);

                // U-Value
                double uValue = Math.Pow(r_tot, -1);
                UValue = Math.Round(uValue, 3);

                QValue = Math.Round(uValue * (Ti - Te), 3);

                // Fehlerabschätzung
                double e = r_tot != 0 ? (r_tot_upper - r_tot_lower) / (2 * r_tot) : 0;
                ErrorEstimation = e * 100;
                ErrorEstimationOk = e * 100 <= 20;
                IsValid = true;
                Logger.LogInfo($"Successfully calculated Inhomogeneous Element: {Element}");
            }
            catch (Exception ex)
            {
                IsValid = false;
                Logger.LogError($"Error calculating Inhomogeneous Element: {Element}, {ex.Message}");
            }
        }

        private void PrepareMappingsForInhomogeneous()
        {
            CreateCalculationAreaBoundaries();

            // Wenn ungültige Bounds ermittelt -> Homogenes Bauteil -> Normale Berechnung des Bauteils!
            if (_calculationAreaBounds.Area == 0)
            {
                CalculateHomogeneous();
                return;
            }

            CreateLayerMapping();
            CreateLayerPathCombinations(0, new List<string>());
            CreateLayerToRectanglesMapping();
            CreateAreaMapping();
            SetAreaWidths();
            SetAreaHeights();
            CreateAreaSharesMapping();
        }
        
        private void CreateLayerPathCombinations(int i, List<string> currentCombination)
        {
            if (i == RelevantLayers.Count)
            {
                _areaToLayerPathCombinations.Add(_areaToLayerPathCombinations.Count + 1, currentCombination);
                return;
            }

            var layer = RelevantLayers[i];
                
            if (layer.HasSubConstructions)
            {
                var newCombinationPath = new List<string>(currentCombination);
                newCombinationPath.Add($"{layer.LayerPosition}b");
                CreateLayerPathCombinations(i + 1, newCombinationPath);
            }

            currentCombination.Add(layer.LayerPosition.ToString());
            CreateLayerPathCombinations(i + 1, currentCombination);
        }

        private void CreateLayerMapping()
        {
            foreach (var layer in RelevantLayers)
            {
                _layerMapping.Add($"{layer.LayerPosition}", layer);
                if (layer.HasSubConstructions)
                {
                    _layerMapping.Add($"{layer.LayerPosition}b", layer.SubConstruction);
                }
            }
        }

        private void CreateAreaMapping()
        {
            foreach (var kvp in _areaToLayerPathCombinations)
            {
                _areaRectangleMapping.Add(kvp.Key, new Rectangle());
            }
        }

        private void CreateLayerToRectanglesMapping()
        {
            foreach (var layerString in _layerMapping.Keys)
            {
                int[] correspondingRectangles = _areaToLayerPathCombinations.Where(kvp => kvp.Value.Contains(layerString)).Select(kvp => kvp.Key).ToArray();
                _layerToAreasMapping.Add(layerString, correspondingRectangles);
            }
        }

        private void CreateCalculationAreaBoundaries()
        {
            var subConstr = RelevantLayers.Where(l => l.SubConstruction != null).Select(l => l.SubConstruction).ToList();
            double maxWidth = 0.0;
            double maxHeight = 0.0;

            if (subConstr.Count > 0)
            {
                var vSubs = subConstr.Where(s => s.SubConstructionDirection == SubConstructionDirection.Vertical).ToList();
                if (vSubs.Count > 0) maxWidth = vSubs.Max(s => s.Width + s.Spacing);

                var hSubs = subConstr.Where(s => s.SubConstructionDirection == SubConstructionDirection.Horizontal).ToList();
                if (hSubs.Count > 0) maxHeight = hSubs.Max(s => s.Width + s.Spacing);
            }
            //  whether exactly ONE of maxWidth or maxHeight is equal to zero
            if (maxWidth == 0.0 ^ maxHeight == 0.0)
            {
                _calculationAreaBounds = new Rectangle(0, 0, Math.Max(maxWidth, maxHeight), Math.Max(maxWidth, maxHeight));
            }
            else if (maxHeight == 0.0 && maxHeight == 0.0)
            {
                _calculationAreaBounds = new Rectangle();
            }
            else
            {
                _calculationAreaBounds = new Rectangle(0, 0, maxWidth, maxHeight);
            }
        }

        // Via Cross Section
        private void SetAreaWidths()
        {
            foreach (var kvp in _areaToLayerPathCombinations)
            {
                var areaRectangle = kvp.Key;
                var layerPathCombos = kvp.Value;

                double currentMinWidth = _calculationAreaBounds.Width; // Starting with maximum Width possible
                
                foreach (var layerString in layerPathCombos)
                {
                    // Check the Type: Use the is keyword to check the type or the as keyword to attempt a cast.
                    if (_layerMapping[layerString] is Layer layer)
                    {
                        // Check: has vertical sub constr
                        if (layer.SubConstruction?.SubConstructionDirection == SubConstructionDirection.Vertical) currentMinWidth = Math.Min(layer.SubConstruction.Spacing, currentMinWidth);
                    }
                    else if (_layerMapping[layerString] is LayerSubConstruction layerSubConstr)
                    {
                        // Check: has vertical sub constr
                        if (layerSubConstr.SubConstructionDirection == SubConstructionDirection.Vertical) currentMinWidth = Math.Min(layerSubConstr.Width, currentMinWidth);
                    }
                    else throw new Exception("Typ konnte nicht aufgelöst werden");
                }
                _areaRectangleMapping[areaRectangle] = new Rectangle(0, 0, currentMinWidth, _areaRectangleMapping[areaRectangle].Height);
            }
        }

        // Via Vertical Cut
        private void SetAreaHeights()
        {
            foreach (var kvp in _areaToLayerPathCombinations)
            {
                var areaRectangle = kvp.Key;
                var layerPathCombos = kvp.Value;

                double currentMinHeight = _calculationAreaBounds.Height; // Starting with maximum Height possible

                foreach (var layerString in layerPathCombos)
                {
                    // Check the Type: Use the is keyword to check the type or the as keyword to attempt a cast.
                    if (_layerMapping[layerString] is Layer layer)
                    {
                        // Check: has horizontal sub constr
                        if (layer.SubConstruction?.SubConstructionDirection == SubConstructionDirection.Horizontal) currentMinHeight = Math.Min(layer.SubConstruction.Spacing, currentMinHeight);
                    }
                    else if (_layerMapping[layerString] is LayerSubConstruction layerSubConstr)
                    {
                        // Check: has horizontal sub constr
                        if (layerSubConstr.SubConstructionDirection == SubConstructionDirection.Horizontal) currentMinHeight = Math.Min(layerSubConstr.Width, currentMinHeight);
                    }
                    else throw new Exception("Typ konnte nicht aufgelöst werden");
                }
                _areaRectangleMapping[areaRectangle] = new Rectangle(0, 0, _areaRectangleMapping[areaRectangle].Width, currentMinHeight);
            }
        }

        private void CreateAreaSharesMapping()
        {
            var hundredPercentArea = _calculationAreaBounds.Area;
            foreach (var kvp in _areaRectangleMapping)
            {
                var propotionalAreaShare = kvp.Value.Area / hundredPercentArea;
                _areaSharesMapping.Add(kvp.Key, propotionalAreaShare);
            }
        }
    }
}
