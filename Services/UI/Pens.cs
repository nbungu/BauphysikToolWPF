using System.Windows.Media;

namespace BauphysikToolWPF.Services.UI
{
    public static class Pens
    {
        public static Pen GetDottedPen(Color color, double thickness)
        {
            return new Pen(new SolidColorBrush(color), thickness)
            {
                DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
            };
        }
        public static Pen GetDottedPen(Brush color, double thickness)
        {
            return new Pen(color, thickness)
            {
                DashStyle = new DashStyle(new double[] { 2, 2 }, 0)
            };
        }
        public static Pen GetDashedPen(Brush color, double thickness)
        {
            return new Pen(color, thickness)
            {
                DashStyle = new DashStyle(new double[] { 12, 12 }, 0)
            };
        }

        public static Pen GetSolidPen(Brush color, double thickness)
        {
            return new Pen(color, thickness);
        }
    }
}
