namespace Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using Olive;
    using System.Net;
    using System.Net.Sockets;
    using Olive.Entities;
    using Olive.Mvc;

    public class SharedActionsController : BaseController
    {
        [Route("healthcheck")]
        public async Task<ActionResult> HealthCheck()
        {
            var myIps = Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(x => x.ToString()).ToString(" | ");

            return Ok($"Health check @ {LocalTime.Now.ToLongTimeString()}, version = {Config.Get("App.Resource.Version")} in env:{Context.Current.Environment().EnvironmentName}, local IP:" + myIps);
        }
    
        [Route("error")]
        public ActionResult Error() => View("error");

        [Route("error/404")]
        public ActionResult NotFound404() => View("error-404");

        [HttpPost, Authorize, Route("upload")]
        public async Task<IActionResult> UploadTempFileToServer(IFormFile[] files)
        {
            return Json(await new FileUploadService().TempSaveUploadedFile(files[0]));
        }

        [HttpGet, Route("file")]
        public async Task<ActionResult> DownloadFile()
        {
            var path = Request.QueryString.ToString().TrimStart('?');
            var accessor = await FileAccessor.Create(path, User);
            if (!accessor.IsAllowed()) return new UnauthorizedResult();

            if (accessor.Blob.IsMedia())
                return await RangeFileContentResult.From(accessor.Blob);
            else return await File(accessor.Blob);
        }

        [Route("temp-file/{key}")]
        public Task<ActionResult> DownloadTempFile(string key) => TempFileService.Download(key);

        [Route("/login")]
        public async Task<ActionResult> Login() => Redirect(Microservice.Of("auth").Url());
    }
}
