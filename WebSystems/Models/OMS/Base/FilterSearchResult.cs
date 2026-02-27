using System;
using Newtonsoft.Json;

namespace WebSystems.Models.OMS.Base
{
    public class FilterSearchResult<T>
    {
        [JsonProperty(PropertyName = "totalCount")]
        public int TotalCount { get; set; }

        [JsonProperty(PropertyName = "results")]
        public T[] Results { get; set; }

        [JsonProperty(PropertyName = "result")]
        public T[] Result { get; set; }
    }
}
