using System;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Stimulsoft.Report;
using Stimulsoft.Report.Mvc;

namespace StimulsoftServer.Controllers
{
    namespace STP.StaRUG.Portal.Controllers
    {
        [AllowAnonymous]
        [Route("api/razor")]
        public class RazorController : Controller
        {
            private readonly IMemoryCache _memoryCache;

            public RazorController(IMemoryCache memoryCache)
            {
                _memoryCache = memoryCache;
            }

            [HttpGet]
            public IActionResult Index([FromQuery] Guid template)
            {
                return View();
            }

            [HttpGet("GetReport")]
            [HttpPost("GetReport")]
            public IActionResult GetReport([FromQuery] Guid template)
            {
                var stiReport = StiReport.CreateNewReport();

                if (_memoryCache.TryGetValue<byte[]>(template.ToString("D"), out var data)) stiReport.Load(data);

                stiReport.RegBusinessObject("Data", new
                {
                    Test = "Test"
                });
                stiReport.Dictionary.SynchronizeBusinessObjects(5);
                stiReport.Dictionary.Synchronize();

                return StiNetCoreDesigner.GetReportResult(this, stiReport);
            }

            [HttpGet("DesignerEvent")]
            [HttpPost("DesignerEvent")]
            public IActionResult DesignerEvent()
            {
                return StiNetCoreDesigner.DesignerEventResult(this);
            }

            [HttpGet("bootstrap.css")]
            public IActionResult GetBootstrap()
            {
                return File(typeof(RazorController).Assembly
                        .GetManifestResourceStream("StimulsoftServer.Resources.bootstrap.min.css"),
                    "text/css");
            }

            [HttpGet("ExportReport")]
            [HttpPost("ExportReport")]
            public IActionResult ExportReport([FromQuery] Guid template)
            {
                var sync = HttpContext.Features.Get<IHttpBodyControlFeature>();

                if (sync != null)
                    sync.AllowSynchronousIO = true;

                var report = StiNetCoreDesigner.GetActionReportObject(this);
                return StiNetCoreDesigner.ExportReportResult(this, report);
            }

            [HttpGet("PreviewReport")]
            [HttpPost("PreviewReport")]
            public IActionResult PreviewReport([FromQuery] Guid template)
            {
                var sync = HttpContext.Features.Get<IHttpBodyControlFeature>();

                if (sync != null)
                    sync.AllowSynchronousIO = true;


                var stiReport = StiNetCoreDesigner.GetReportObject(this);

                if (stiReport != null)
                {
                    stiReport.RegBusinessObject("Data", new
                    {
                        Test = "Test"
                    });
                    stiReport.Dictionary.SynchronizeBusinessObjects(5);
                    stiReport.Dictionary.Synchronize();
                }

                return StiNetCoreDesigner.PreviewReportResult(this, stiReport);
            }

            [HttpPost("SaveReport")]
            public IActionResult SaveReport([FromQuery] Guid template)
            {
                var report = StiNetCoreDesigner.GetReportObject(this);

                using var ms = new MemoryStream();
                report.Save(ms);
                ms.Position = 0;

                _memoryCache.Set(template.ToString("D"), ms.ToArray());

                return StiNetCoreDesigner.SaveReportResult(this);
            }
        }
    }
}