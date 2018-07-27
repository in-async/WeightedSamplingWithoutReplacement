namespace System.Diagnostics {

    public static class ObjectExtensions {

        public static void Dump<T>(this T source) => Trace.WriteLine(source);
    }
}