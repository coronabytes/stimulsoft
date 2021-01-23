using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
            public async Task<IActionResult> GetReport([FromQuery] Guid template)
            {
                var stiReport = StiReport.CreateNewReport();

                if (_memoryCache.TryGetValue<byte[]>(template.ToString("D"), out var data)) stiReport.Load(data);

                stiReport.RegBusinessObject("Data", new
                {
                    Test = "Test"
                });
                stiReport.Dictionary.SynchronizeBusinessObjects(5);
                await stiReport.Dictionary.SynchronizeAsync();

                return await StiNetCoreDesigner.GetReportResultAsync(this, stiReport);
            }

            [HttpGet("DesignerEvent")]
            [HttpPost("DesignerEvent")]
            public async Task<IActionResult> DesignerEvent()
            {
                return await StiNetCoreDesigner.DesignerEventResultAsync(this);
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
            public async Task<IActionResult> ExportReport([FromQuery] Guid template)
            {
                var report = StiNetCoreDesigner.GetActionReportObject(this);
                return await StiNetCoreDesigner.ExportReportResultAsync(this, report);
            }

            [HttpGet("PreviewReport")]
            [HttpPost("PreviewReport")]
            public async Task<IActionResult> PreviewReport([FromQuery] Guid template)
            {
                var stiReport = StiNetCoreDesigner.GetReportObject(this);

                if (stiReport != null)
                {
                    stiReport.RegBusinessObject("Data", new
                    {
                        Test = "Test"
                    });
                    stiReport.Dictionary.SynchronizeBusinessObjects(5);
                    await stiReport.Dictionary.SynchronizeAsync();
                }

                return await StiNetCoreDesigner.PreviewReportResultAsync(this, stiReport);
            }

            [HttpPost("SaveReport")]
            public async Task<IActionResult> SaveReport([FromQuery] Guid template)
            {
                var report = StiNetCoreDesigner.GetReportObject(this);

                await using var ms = new MemoryStream();
                report.Save(ms);
                ms.Position = 0;

                _memoryCache.Set(template.ToString("D"), ms.ToArray());

                return await StiNetCoreDesigner.SaveReportResultAsync(this);
            }
        }
    }
}