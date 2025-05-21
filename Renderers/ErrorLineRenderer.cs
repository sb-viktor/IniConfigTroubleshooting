using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using AvaloniaEdit;

namespace IniConfigTroubleshooting.Renderers;

public class ErrorLineRenderer : IBackgroundRenderer
{
    private readonly TextDocument _doc;
    private int _startLine;
    private int _endLine;
    public ErrorLineRenderer(TextDocument doc, int startLine = 0, int endLine = 0)
    {
        _doc = doc;
        SetLines(startLine, endLine);
    }
    public void SetLines(int startLine, int endLine)
    {
        _startLine = startLine;
        _endLine = endLine;
    }
    public KnownLayer Layer => KnownLayer.Selection;
    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (_doc.LineCount == 0 || _startLine > _endLine)
            return;
        int start = Math.Max(1, _startLine);
        int end = Math.Min(_doc.LineCount, _endLine);
        for (int line = start; line <= end; line++)
        {
            var docLine = _doc.GetLineByNumber(line);
            foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, new TextSegment { StartOffset = docLine.Offset, Length = docLine.Length }))
            {
                DrawErrorUnderline(drawingContext, rect, Colors.Red);
            }
        }
    }
    private static void DrawErrorUnderline(DrawingContext dc, Avalonia.Rect rect, Color color)
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
