using System.Collections.Generic;

namespace PryGuard.Core.Browser.Model.Configs;
public class FakeProfileComparer : IEqualityComparer<FakeProfile>
{
    public bool Equals(FakeProfile x, FakeProfile y)
    {
        return x == y || x != null && y != null
            && x.BaseLatency == y.BaseLatency && !(x.CanvasFingerPrintHash != y.CanvasFingerPrintHash)
            && x.BrowserTypeType == y.BrowserTypeType && x.OsVersion == y.OsVersion && x.IsX64 == y.IsX64;
    }

    public int GetHashCode(FakeProfile obj)
    {
        return obj.GetHashCode();
    }
}
