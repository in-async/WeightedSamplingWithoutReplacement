namespace Newtonsoft.Json {

    public static class ObjectExtensions {

        public static string ToJson(this object obj) => JsonConvert.SerializeObject(obj, Formatting.Indented);
    }
}