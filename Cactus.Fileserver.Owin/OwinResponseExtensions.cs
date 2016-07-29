using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Cactus.Fileserver.Owin
{
    public static class OwinResponseExtensions
    {
        public static async Task ResponseOk(this IOwinResponse response, object data)
        {
            if (data == null)
            {
                response.StatusCode = 204;
                return;
            }

            response.StatusCode = 200;
            response.ContentType = "application/json";
            var writter = new StreamWriter(response.Body);
            await writter.WriteAsync(JsonConvert.SerializeObject(data));
            await writter.FlushAsync();
        }
    }
}
