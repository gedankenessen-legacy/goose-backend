using Microsoft.AspNetCore.Mvc.ModelBinding;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Goose.Data
{
    public class ObjectIdBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
            // TODO validate
            bindingContext.Result = ModelBindingResult.Success(new ObjectId(result.FirstValue));
            return Task.CompletedTask;
        }
    }
    public class ObjectIdBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(ObjectId))
            {
                return new ObjectIdBinder();
            }

            // no binder found
            return null;
        }
    }

}
