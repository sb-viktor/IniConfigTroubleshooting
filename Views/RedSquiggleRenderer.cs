using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit;

namespace IniConfigTroubleshooting.Views;

public class RedSquiggleRenderer : IBackgroundRenderer
{
    private readonly int _length;
    private readonly TextDocument _doc;
    public RedSquiggleRenderer(TextDocument doc)
    {
        _doc = doc;
        _length = doc.TextLength;
    }
    public KnownLayer Layer => KnownLayer.Selection;
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_length == 0)
        {
            return;
        }

        foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, new TextSegment { StartOffset = 0, Length = _length }))
        {
            DrawSquigglyUnderline(drawingContext, rect, Colors.Red);
        }
    }
    private static void DrawSquigglyUnderline(DrawingContext dc, Avalonia.Rect rect, Color color)
    {
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            var y = rect.Bottom - 2;
            var x = rect.Left;
            var up = true;
            ctx.BeginFigure(new Avalonia.Point(x, y), false);
            while (x < rect.Right)
            {
                x += 2;
                y += up ? -2 : 2;
                ctx.LineTo(new Avalonia.Point(x, y));
                up = !up;
            }
        }
        dc.DrawGeometry(null, new Pen(new SolidColorBrush(color), 1), geo);
    }
    public void Attach(TextEditor editor)
    {
        editor.TextArea.TextView.BackgroundRenderers.Add(this);
        editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }
    public void Detach(TextEditor editor)
    {
        _ = editor.TextArea.TextView.BackgroundRenderers.Remove(this);
        editor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
    }
}
