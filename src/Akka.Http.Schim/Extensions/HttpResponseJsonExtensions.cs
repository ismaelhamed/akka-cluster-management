using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpResponseJsonExtensions
    {
        /// <summary>
        /// Write the specified value as JSON to the response body. The response content-type will be set to <c>application/json; charset=utf-8</c>.
        /// </summary>
        /// <param name="response">The response to write JSON to.</param>
        /// <param name="json">The value to write as JSON.</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the operation.</param>
        public static async Task WriteAsJsonAsync(this HttpResponse response, string json, CancellationToken cancellationToken = default)
        {
            // Set the content type
            response.ContentType = "application/json; charset=utf-8";

            // Write the content
            await response.WriteAsync(json, cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Write the specified value as JSON to the response body. The response content-type will be set to <c>application/json; charset=utf-8</c>.
        /// </summary>
        /// <param name="response">The response to write JSON to.</param>
        /// <param name="value">The value to write as JSON.</param>
        /// <param name="cancellationToken">A CancellationToken used to cancel the operation.</param>
        public static async Task WriteAsJsonAsync<TValue>(this HttpResponse response, TValue value, CancellationToken cancellationToken = default)
        {
            // Set the content type
            response.ContentType = "application/json; charset=utf-8";

            // Write the content
            await response.WriteAsync(JsonConvert.SerializeObject(value), cancellationToken);
            await response.Body.FlushAsync(cancellationToken);
        }
    }
}
