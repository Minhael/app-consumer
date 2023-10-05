using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using Common.Lang.Extensions;

namespace Common.Telemetry.OpenTelemetry;

public static class OTelEnrichers
{
    public static Action<Activity, string, object> CombineWith(params Action<Activity, string, object>[] enrichers) => (activity, name, obj) =>
    {
        foreach (var enricher in enrichers)
        {
            enricher(activity, name, obj);
        }
    };

    public static Action<Activity, string, object> SqlCommandDbParameters => (activity, name, obj) =>
    {
        if (obj is SqlCommand sqlCommand)
        {
            Dictionary<string, string?> parameters = new Dictionary<string, string?>();
            foreach (DbParameter parameter in sqlCommand.Parameters)
                parameters[parameter.ParameterName] = parameter.Value?.ToString();
            activity.SetTag("db.parameters", parameters.OrderBy(x => x.Value?.Length ?? 0)
                                                       .Select(x => $"@{x.Key}={x.Value}")
                                                       .JoinString(", "));
        }
    };
}