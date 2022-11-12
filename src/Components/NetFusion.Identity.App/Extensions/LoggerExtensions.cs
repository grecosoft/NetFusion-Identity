using Microsoft.Extensions.Logging;
using NetFusion.Identity.Domain.Validation;
using Serilog.Context;

namespace NetFusion.Identity.App.Extensions;

/// <summary>
/// Logger extensions for logging domain validation state.
/// </summary>
public static class LoggerExtensions
{
    public static void LogValidations(this ILogger logger,
        string validationContext,
        IHasValidations entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        LogValidations(logger, validationContext, entity.Validations);
    }

    public static void LogValidations(this ILogger logger, 
        string validationContext,
        DomainValidations validations)
    {
        if (validations.Valid) return;

        var errors = validations.Items
            .Where(v => v.Level == ValidationLevel.Error)
            .Select(v => v.Message)
            .ToArray();

        var warnings = validations.Items
            .Where(v => v.Level == ValidationLevel.Warning)
            .Select(v => v.Message)
            .ToArray();

        using (LogContext.PushProperty("Errors", errors, destructureObjects: true))
        using (LogContext.PushProperty("Warnings", warnings, destructureObjects: true))
        {
            logger.LogInformation("Domain Validations Recorded for {Context}", validationContext);
        }
    }
}