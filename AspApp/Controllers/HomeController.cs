using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;
using AspApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace AspApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public HomeController(IHttpClientFactory httpClientFactory, IWebHostEnvironment environment)
        {
            _httpClient = httpClientFactory.CreateClient();
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            string ApiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";
            var response = await _httpClient.GetAsync(ApiUrl);
            if (!response.IsSuccessStatusCode)
            {
                return View(new List<Employee>());
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(response);
            var entries = JsonSerializer.Deserialize<List<Employee>>(json);

            if (entries == null)
            {
                ViewBag.Error = "No data received from API.";
                return View(new List<EmployeeTotalWorkTime>());
            }

            var totals = entries
                .Where(e => e.DeletedOn == null)
                .Where(e => e.EmployeeName != null)
                .GroupBy(e => e.EmployeeName)
                .Select(et => new EmployeeTotalWorkTime
                {
                    EmployeeName = et.Key,
                    TotalHours = et.Sum(e =>
                    {
                        var duration = (e.EndTimeUtc - e.StarTimeUtc).TotalHours;
                        return Math.Max(0, duration);
                    })
                })
                .OrderByDescending(x => x.TotalHours)
                .ToList();

            var chartPath = GeneratePieChart(totals);
            ViewBag.ChartPath = chartPath;
            
            return View(totals);
        }

        public IActionResult GetPieChart()
        {
            try
            {
                var chartBytes = System.IO.File.ReadAllBytes(GetChartFilePath());
                return File(chartBytes, "image/png");
            }
            catch
            {
                return File(new byte[0], "image/png");
            }
        }

        private string GeneratePieChart(List<EmployeeTotalWorkTime> employees)
        {
            if (employees == null || !employees.Any())
                return string.Empty;


            var width = 800;
            var height = 600;

            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.Clear(Color.White);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var titleFont = new Font("Arial", 16, FontStyle.Bold);
            graphics.DrawString("Total hours worked by employee", titleFont, Brushes.Black, 50, 10);

            var totalHours = employees.Sum(e => e.TotalHours);
            var startAngle = 0f;
            var random = new Random();
            var colors = new List<Color>();

            var rect = new Rectangle(50, 50, 400, 400);

            for(int i = 0; i < employees.Count; i++)
            {
                colors.Add(Color.FromArgb(
                    random.Next(150, 256),
                    random.Next(150, 256),
                    random.Next(150, 256)
                    ));
            }

            for(int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];
                var sweepAngle = (float)(employee.TotalHours / totalHours * 360);

                using var brush = new SolidBrush(colors[i]);
                graphics.FillPie(brush, rect, startAngle, sweepAngle);
                graphics.DrawPie(Pens.Black, rect, startAngle, sweepAngle);

                startAngle += sweepAngle;
            }

            var legendY = 50;
            var legendFont = new Font("Arial", 10);

            for(int i = 0; i < employees.Count; i++)
            {
                var employee = employees[i];
                var percent = (employee.TotalHours / totalHours * 100).ToString("0.0");
                var text = $"{employee.EmployeeName} - {percent}% ({employee.TotalHours:0} hours";

                using var colorBrush = new SolidBrush(colors[i]);
                graphics.FillRectangle(colorBrush, 500, legendY, 15, 15);
                graphics.DrawRectangle(Pens.Black, 500, legendY, 15, 15);

                graphics.DrawString(text, legendFont, Brushes.Black, 520, legendY - 2);
                legendY += 25;
            }

            var chartPath = GetChartFilePath();
            bitmap.Save(chartPath, ImageFormat.Png);

            return Path.GetFileName(chartPath);
        }

        private string GetChartFilePath()
        {
            var wwwrootPath = _environment.WebRootPath;
            var chartsFolder = Path.Combine(wwwrootPath, "charts");

            if (!Directory.Exists(chartsFolder))
            {
                Directory.CreateDirectory(chartsFolder);
            }
            return Path.Combine(chartsFolder, "employee_pie_chart.png");
        }
    }
}
