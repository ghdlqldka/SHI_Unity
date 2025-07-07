using Newtonsoft.Json.Linq;

namespace _Base_Framework
{
    public static class _JsonUtils
    {
        public static string ToJson(string data)
        {
            // Debug.LogFormat(LOG_FORMAT, "data : " + data);

            JObject jo = JObject.Parse(data);
            JToken jtoken = jo;

            return jtoken.ToString();
        }
    }
}
