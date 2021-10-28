using Verse;

namespace FactionColonies.util
{
    public class Curves
    {
        public static SimpleCurve RandomEventCurve = new SimpleCurve()
        {
            new CurvePoint(0f, 0f),
            new CurvePoint(0.25f, 0.125f),
            new CurvePoint(0.5f, 0.25f),
            new CurvePoint(0.75f, 0.5f),
            new CurvePoint(1f, 1f)
        };
    }
}
