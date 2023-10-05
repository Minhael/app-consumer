# Toggle

This package provides a feature toggle framework inspried by [feature toggles](https://martinfowler.com/articles/feature-toggles.html).

# Permission format

```
scope:this-permission-name

For example,
persona:my-day-is-allow-ghosting
```

> Scope is a unique name to identify set of permissions.

# Principle

To make a decision on whether the application should switch the behaviour, `IControl` first requests list of `Permissions` from `IAuthorizer`. Then, feed them into the `IPolicy` that responsible to make the decision.

```csharp
public record SomeSystemProcessDecision
{
    public bool IsFeature1TurnedOn { get; init; }
    public int Feature2Threshold { get; init; }
}

public class SomeSystemProcessPolicy : Policy<SomeSystemProcessDecision>
{
    public override SomeSystemProcessDecision Create(Permissions permissions)
    {
        return new SomeSystemProcessDecision
        {
            IsFeature1TurnedOn = permissions.GetValue<bool>("modifier:feature1") ?? false,
            Feature2Threshold = permissions.GetValue<int>("modifier:feature2") ?? 0,
        };
    }
}

private readonly IControl _control;

public void SomeProcess() {
    var decision = await control.Decide<SomeSystemProcessDecision>();
    result.IsFeature1TurnedOn.Should().BeTrue();
    result.Feature2Threshold.Should().Be(5);
}
```
