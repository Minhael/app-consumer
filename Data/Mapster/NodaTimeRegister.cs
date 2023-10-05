using Mapster;
using NodaTime;
using NodaTime.Text;

namespace Common.Data.Mapster;

public class NodaTimeRegister : IRegister
{
    public static void RegisterGlobally()
    {
        TypeAdapterConfig.GlobalSettings.Apply(new NodaTimeRegister());
    }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<string?, Instant?>().MapWith(x => x != null ? InstantPattern.ExtendedIso.Parse(x).Value : null);
        config.NewConfig<string, Instant>().MapWith(x => InstantPattern.ExtendedIso.Parse(x).Value);
        config.NewConfig<string?, LocalDate?>().MapWith(x => x != null ? LocalDatePattern.Iso.Parse(x).Value : null);
        config.NewConfig<string, LocalDate>().MapWith(x => LocalDatePattern.Iso.Parse(x).Value);
        config.NewConfig<string?, LocalDateTime?>().MapWith(x => x != null ? LocalDateTimePattern.ExtendedIso.Parse(x).Value : null);
        config.NewConfig<string, LocalDateTime>().MapWith(x => LocalDateTimePattern.ExtendedIso.Parse(x).Value);
        config.NewConfig<string?, OffsetDateTime?>().MapWith(x => x != null ? OffsetDateTimePattern.GeneralIso.Parse(x).Value : null);
        config.NewConfig<string, OffsetDateTime>().MapWith(x => OffsetDateTimePattern.GeneralIso.Parse(x).Value);
        config.NewConfig<DateTime, Instant>().MapWith(x => LocalDateTime.FromDateTime(x).InUtc().ToInstant());
        config.NewConfig<DateTime, LocalDate>().MapWith(x => LocalDate.FromDateTime(x));
        config.NewConfig<DateTime, LocalDateTime>().MapWith(x => LocalDateTime.FromDateTime(x));
        config.NewConfig<DateTimeOffset, OffsetDateTime>().MapWith(x => OffsetDateTime.FromDateTimeOffset(x));
        config.NewConfig<DateTime?, Instant?>().MapWith(x => x != null ? LocalDateTime.FromDateTime((DateTime)x).InUtc().ToInstant() : null);
        config.NewConfig<DateTime?, LocalDate?>().MapWith(x => x != null ? LocalDate.FromDateTime((DateTime)x) : null);
        config.NewConfig<DateTime?, LocalDateTime?>().MapWith(x => x != null ? LocalDateTime.FromDateTime((DateTime)x) : null);
        config.NewConfig<DateTimeOffset?, OffsetDateTime?>().MapWith(x => x != null ? OffsetDateTime.FromDateTimeOffset((DateTimeOffset)x) : null);
    }
}