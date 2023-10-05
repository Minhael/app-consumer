namespace Common.Lang.Monad;

public readonly struct Either<L, R>
{
    internal readonly R? right;
    internal readonly L? left;
    public readonly EitherStatus State;

    private bool isLeft => EitherStatus.IsLeft == State;
    private bool isRight => EitherStatus.IsRight == State;

    private Either(R right)
    {
        if (right is null) throw new NullReferenceException();

        this.State = EitherStatus.IsRight;
        this.right = right;
        this.left = default(L);
    }

    private Either(L left)
    {
        if (left is null) throw new NullReferenceException();
        this.State = EitherStatus.IsLeft;
        this.right = default(R);
        this.left = left;
    }

    public object? UntypedValue => State switch
    {
        EitherStatus.IsRight => right,
        EitherStatus.IsLeft => left,
        _ => null
    };

    public static implicit operator Either<L, R>(R right)
    {
        return new Either<L, R>(right);
    }

    public static implicit operator Either<L, R>(L left)
    {
        return new Either<L, R>(left);
    }

    public static explicit operator L(Either<L, R> ma) =>
            ma.isLeft && ma.left != null
                ? ma.left
                : throw new InvalidCastException("Either is not in a Left state");

    public static explicit operator R(Either<L, R> ma) =>
            ma.isRight && ma.right != null
                ? ma.right
                : throw new InvalidCastException("Either is not in a Right state");

    public TResult Match<TResult>(Func<L, TResult> left, Func<R, TResult> right)
    {
        if (this.isLeft && this.left is not null)
            return left(this.left);

        if (this.isRight && this.right is not null)
            return right(this.right);

        throw new NullReferenceException();
    }
}

public enum EitherStatus : byte
{
    IsLeft = 1,
    IsRight = 2
}

public static class EitherExtension
{
    public static L[] ToLeftArray<L, R>(this Either<L, R>[] eithers)
    {
        return eithers.Where(x => x.UntypedValue is L).Select(x => (L)x).ToArray();
    }

    public static R[] ToRightArray<L, R>(this Either<L, R>[] eithers)
    {
        return eithers.Where(x => x.UntypedValue is R).Select(x => (R)x).ToArray();
    }
}