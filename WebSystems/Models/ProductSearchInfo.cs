using System;
using Newtonsoft.Json;

namespace WebSystems.Models
{
    public class ProductSearchInfo
    {
        [JsonProperty(PropertyName = "good_id")]
        public int? GoodId { get; set; }

        [JsonProperty(PropertyName = "identified_by")]
        public ProductSearchIdentifiedBy[] IdentifiedBy { get; set; }

        [JsonProperty(PropertyName = "good_name")]
        public string GoodName { get; set; }

        [JsonProperty(PropertyName = "good_img")]
        public string GoodImg { get; set; }

        [JsonProperty(PropertyName = "party_brand_id")]
        public string PartyBrandId { get; set; }

        [JsonProperty(PropertyName = "brand_id")]
        public int? BrandId { get; set; }

        [JsonProperty(PropertyName = "brand_name")]
        public string BrandName { get; set; }

        [JsonProperty(PropertyName = "good_rating")]
        public int? GoodRating { get; set; }

        [JsonProperty(PropertyName = "good_url")]
        public string GoodUrl { get; set; }

        [JsonProperty(PropertyName = "is_kit")]
        public bool? IsKit { get; set; }

        [JsonProperty(PropertyName = "is_set")]
        public bool? IsSet { get; set; }

        [JsonProperty(PropertyName = "good_status")]
        public string GoodStatus { get; set; }

        [JsonProperty(PropertyName = "create_date")]
        public DateTime? CreateDate { get; set; }

        [JsonProperty(PropertyName = "update_date")]
        public DateTime? UpdateDate { get; set; }

        [JsonProperty(PropertyName = "remainder_type")]
        public string RemainderType { get; set; }

        [JsonProperty(PropertyName = "is_tech_gtin")]
        public bool? IsTechGtin { get; set; }

        [JsonProperty(PropertyName = "first_sign_date")]
        public DateTime? FirstSignDate { get; set; }
    }
}
