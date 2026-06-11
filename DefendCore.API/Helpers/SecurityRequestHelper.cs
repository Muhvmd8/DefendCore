namespace DefendCore.API.Helpers
{
    public static class SecurityRequestHelper
    {
        public static bool ShouldInspectRequest(HttpRequest request)
        => HttpMethods.IsPost(request.Method)
            || HttpMethods.IsPut(request.Method)
            || HttpMethods.IsPatch(request.Method);

        public static bool IsPayloadSizeAllowed(
            HttpRequest request,
            int maxScanBodySize)
            => request.ContentLength.HasValue
            && request.ContentLength.Value <= maxScanBodySize;

        public static bool IsSafeContentType(HttpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ContentType))
                return false;

            return request.ContentType.Contains("application/json")
                || request.ContentType.Contains("application/x-www-form-urlencoded")
                || request.ContentType.Contains("text/plain");
        }
    }
}
