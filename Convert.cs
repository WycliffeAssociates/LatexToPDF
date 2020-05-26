using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LatexToPdf
{
    public static class Convert
    {
        [FunctionName("Convert")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string tempDir = CreateTempFolder();
            string latexFilePath = Path.Join(tempDir, "output.tex");
            string pdfFilePath = Path.Join(tempDir, "output.pdf");
            string latexBody = await new StreamReader(req.Body).ReadToEndAsync();
            File.WriteAllText(latexFilePath, latexBody);

            // Convert via pdflatex
            ProcessStartInfo startInfo = new ProcessStartInfo("pdflatex", "-halt-on-error " + latexFilePath){
                WorkingDirectory = tempDir,
            };


            using(var runningProcess = Process.Start(startInfo)){
                log.LogInformation("Starting conversion");
                runningProcess.WaitForExit();
                if (runningProcess.ExitCode != 0){
                    log.LogError("Error rendering pdf", runningProcess.StandardOutput);
                    return new BadRequestObjectResult("Error in processing");
                }
                log.LogInformation("Finished conversion");
            }


            var stream = File.OpenRead(pdfFilePath);
            return new FileStreamResult(stream,"application/octet-stream"){
                FileDownloadName = "output.pdf"
            };
        }
        public static string CreateTempFolder()
        {
            string path = Path.GetTempPath() + Guid.NewGuid();
            Directory.CreateDirectory(path);
            return path;
        }
    }

}
