using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KaiYan.Core.API
{
    internal class VideoHttp: HttpBase
    {
        public static Task<JObject> GetVideo(string url)
        {
            if (url?.Contains("/category/") == true)
            {
                url = url.Replace("/category/", "//category/");
            }
            else if (url?.Contains("/categories/") == true)
            {
                url = url.Replace("/categories/", "//categories/");
            }
            return GETAsync<JObject>(url);
        }
        public static Task<JObject> GetRelatedVideo(string id)
        {
            var url = "https://baobab.kaiyanapp.com/api/v4/video/related";
            var _params = new Dictionary<string, string>();
            _params.Add("id", id);
            return GETAsync<JObject>(url,_params);
        }
    }
}
