namespace Common.Lang.Extensions;

public static class ExceptionExtensions
{
    public static Exception GetRootCause(this Exception self)
    {
        return self.InnerException?.GetRootCause() ?? self;
    }
}