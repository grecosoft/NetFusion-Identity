using Microsoft.AspNetCore.Identity;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.App.Extensions;

/// <summary>
/// Extension methods for populating domain validations from results
/// returned from ASP.NET Core Identity.
/// </summary>
public static class IdentityErrorExtensions
{
    public static void AddValidations(this IHasValidations entity,
        params IdentityResult[] result)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
  
        AddValidations(entity.Validations, result);
    }
    
    public static void AddValidations(this DomainValidations domainValidations,
        params IdentityResult[] result)
    {
        if (domainValidations == null) throw new ArgumentNullException(nameof(domainValidations));
        if (result == null) throw new ArgumentNullException(nameof(result));
        
        var validations = result.SelectMany(r => r.Errors).Select(e => 
            new ValidationResult(ValidationLevel.Error, e.Description));
        
        domainValidations.Append(validations);
    }
}