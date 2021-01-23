using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Stimulsoft.Report;
using Stimulsoft.Report.Angular;
using Stimulsoft.Report.Export;
using Stimulsoft.Report.Mvc;
using Stimulsoft.Report.Web;

namespace StimulsoftServer.Controllers
{
    [ApiController]
    [Route("api/angular")]
    public class AngularController : Controller
    {
        private readonly IMemoryCache _memoryCache;

        public AngularController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Guid? template = null)
        {
            var requestParams = StiNetCoreDesigner.GetRequestParams(this);
            if (requestParams.Action == StiAction.Undefined)
            {
                var options = new StiAngularDesignerOptions
                {
                    Behavior = new StiNetCoreDesignerOptions.BehaviorOptions
                    {
                        ShowSaveDialog = false
                    },
                    Exports = new StiNetCoreDesignerOptions.ExportOptions
                    {
                        ShowExportToCsv = false,
                        ShowExportToDbf = false,
                        ShowExportToDif = false,
                        ShowExportToDocument = false,
                        ShowExportToExcel = false,
                        ShowExportToExcel2007 = true,
                        ShowExportToExcelXml = false,
                        ShowExportToHtml = false,
                        ShowExportToHtml5 = false,
                        ShowExportToImageBmp = false,
                        ShowExportToImageGif = false,
                        ShowExportToImageJpeg = false,
                        ShowExportToImageMetafile = false,
                        ShowExportToImagePcx = false,
                        ShowExportToImagePng = true,
                        ShowExportToImageSvg = true,
                        ShowExportToImageSvgz = false,
                        ShowExportToImageTiff = true,
                        ShowExportToJson = true,
                        ShowExportToMht = false,
                        ShowExportToOpenDocumentCalc = true,
                        ShowExportToOpenDocumentWriter = true,
                        ShowExportToPdf = true,
                        ShowExportToPowerPoint = true,
                        ShowExportToRtf = true,
                        ShowExportToSylk = false,
                        ShowExportToText = false,
                        ShowExportToWord2007 = true,
                        ShowExportToXml = false,
                        ShowExportToXps = false,
                        DefaultSettings = new StiDefaultExportSettings
                        {
                            ExportToPdf =
                            {
                                PdfComplianceMode = StiPdfComplianceMode.A3,
                                AllowEditable = StiPdfAllowEditable.No
                            }
                        }
                    },
                    FileMenu = new StiNetCoreDesignerOptions.FileMenuOptions
                    {
                        ShowExit = false,
                        ShowAbout = false,
                        ShowClose = false,
                        ShowHelp = false,
                        ShowNew = false,
                        ShowInfo = false,
                        ShowOpen = true,
                        ShowOptions = false,
                        ShowSave = true,
                        ShowSaveAs = true,
                        ShowReportSetup = false
                    }
                };
                return StiAngularDesigner.DesignerDataResult(requestParams, options);
            }

            return await StiNetCoreDesigner.ProcessRequestResultAsync(this);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] Guid? template = null)
        {
            var requestParams = StiNetCoreDesigner.GetRequestParams(this);
            if (requestParams.Component == StiComponentType.Designer)
                switch (requestParams.Action)
                {
                    case StiAction.GetReport:
                        return await GetReport(template ?? Guid.Empty);

                    case StiAction.PreviewReport:
                        return await GetPreview(template ?? Guid.Empty);

                    case StiAction.ExportReport:
                        return await ExportReport(template ?? Guid.Empty);

                    case StiAction.SaveReport:
                        return await SaveReport(template ?? Guid.Empty);
                }

            return await StiNetCoreDesigner.ProcessRequestResultAsync(this);
        }

        private async Task<IActionResult> GetReport(Guid key)
        {
            var stiReport = StiReport.CreateNewReport();

            if (_memoryCache.TryGetValue<byte[]>(key.ToString("D"), out var data))
                stiReport.Load(data);

            stiReport.RegBusinessObject("Data", new
            {
                Test = "Test"
            });
            stiReport.Dictionary.SynchronizeBusinessObjects(5);
            await stiReport.Dictionary.SynchronizeAsync();

            return await StiNetCoreDesigner.GetReportResultAsync(this, stiReport);
        }

        private async Task<IActionResult> GetPreview(Guid key)
        {
            var stiReport = StiNetCoreDesigner.GetReportObject(this);

            stiReport.RegBusinessObject("Data", new
            {
                Test = "Test"
            });
            stiReport.Dictionary.SynchronizeBusinessObjects(5);
            await stiReport.Dictionary.SynchronizeAsync();

            return await StiNetCoreDesigner.PreviewReportResultAsync(this, stiReport);
        }

        private async Task<IActionResult> SaveReport(Guid key)
        {
            var report = StiNetCoreDesigner.GetReportObject(this);

            await using var ms = new MemoryStream();
            report.Save(ms);
            ms.Position = 0;

            _memoryCache.Set(key.ToString("D"), ms.ToArray());

            return await StiNetCoreDesigner.SaveReportResultAsync(this);
        }

        private async Task<IActionResult> ExportReport(Guid key)
        {
            var report = StiNetCoreDesigner.GetReportObject(this);
            return await StiNetCoreDesigner.ExportReportResultAsync(this, report);
        }
    }
}