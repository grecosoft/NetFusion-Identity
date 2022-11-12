using Microsoft.AspNetCore.Mvc.ModelBinding;
using NetFusion.Identity.Domain.Validation;

namespace NetFusion.Identity.Client.Models
{
    public static class ModelStateExtensions
    {
        public static void AddValidations(this ModelStateDictionary modelState,
            DomainValidations validation)
        {
            foreach(ValidationResult result in validation.Items)
            {
                modelState.AddModelError("", result.Message);
            }
        }
    }
}

