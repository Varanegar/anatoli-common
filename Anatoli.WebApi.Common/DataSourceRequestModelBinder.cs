using Anatoli.Common.DynamicLinq;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Anatoli.Common.WebApi
{
    public class DataSourceRequestModelBinder : IModelBinder
    {
        public enum GridUrlParameters
        {
            Sort,
            Page,
            PageSize,
            Filter,
            Group,
            Aggregates
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            var request = new DataSourceRequest();

            string sort, group, filter, aggregates;
            int currentPage;
            int pageSize;

            if (TryGetValue(bindingContext, GridUrlParameters.Sort.ToString(), out sort))
            {
                //request.Sorts = GridDescriptorSerializer.Deserialize<SortDescriptor>(sort);
            }

            if (TryGetValue(bindingContext, GridUrlParameters.Page.ToString(), out currentPage))
            {
                request.Page = currentPage;
            }

            if (TryGetValue(bindingContext, GridUrlParameters.PageSize.ToString(), out pageSize))
            {
                request.PageSize = pageSize;
            }

            if (TryGetValue(bindingContext, GridUrlParameters.Filter.ToString(), out filter))
            {
               // request.Filters = FilterDescriptorFactory.Create(filter);
            }

            //if (TryGetValue(bindingContext, GridUrlParameters.Group.ToString(), out group))
            //{
            //    request.Groups = GridDescriptorSerializer.Deserialize<GroupDescriptor>(group);
            //}

            //if (TryGetValue(bindingContext, GridUrlParameters.Aggregates.ToString(), out aggregates))
            //{
            //    request.Aggregates = GridDescriptorSerializer.Deserialize<AggregateDescriptor>(aggregates);
            //}

            bindingContext.Model = request;
            return true;
        }

        private bool TryGetValue<T>(ModelBindingContext bindingContext, string key, out T result)
        {
            var value = bindingContext.ValueProvider.GetValue(key);

            if (value == null)
            {
                result = default(T);

                return false;
            }

            result = (T)value.ConvertTo(typeof(T));

            return true;
        }
    }
}
