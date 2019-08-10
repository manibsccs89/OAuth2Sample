using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace AuthServer.Infrastructure.Extensions
{
    public static class ModelStateExtensions
    {
        public static List<string> GetErrorsList(this ModelStateDictionary dictionary)
        {
            var errorMessages = new List<string>();

            List<ModelErrorCollection> errors = dictionary
                .Select(x => x.Value.Errors)
                .ToList();

            errors.ForEach(error => errorMessages
                .AddRange(error.Select(b => b.ErrorMessage)));

            return errorMessages;
        }
    }
}