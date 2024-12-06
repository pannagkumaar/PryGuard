namespace PryGuard.Core.Browser.Model.Configs;
public class ScreenSize
{
    public ScreenSize(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public int Height { get; private set; }

    public int Width { get; private set; }

    public int WindowOuterHeight => Height - 40;

    public int WindowOuterWidth => Width;

    public int ScreenAvailHeight => Height - 40;

    public int WindowScreenAvailWidth => Width;

    public int WindowScreenHeight => Height;

    public int WindowScreenWidth => Width;

    public override string ToString()
    {
        return $"{Width} x {Height}";
    }

    public override int GetHashCode()
    {
        return Width ^ Height;
    }

    public override bool Equals(object obj)
    {
        if (obj != null && obj.GetType() != GetType())
        {
            return base.Equals(obj);
        }
        var v = (ScreenSize)obj;
        return v.Width == Width && v.Height == Height;
    }
}