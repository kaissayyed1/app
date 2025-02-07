using api.Repositories;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Pdf;
using iText.Html2pdf;
using Humanizer;
using System.Globalization;

namespace api.Controllers
{
    [Route("api/payslip")]
    [ApiController]
    public class PayslipController : ControllerBase
    {
        private readonly IPayslipRepository _payslipRepository;

        public PayslipController(IPayslipRepository payslipRepository)
        {
            _payslipRepository = payslipRepository;
        }

        [HttpGet("years")]
        public async Task<IActionResult> GetYears()
        {
            var years = await _payslipRepository.GetYears();
            return Ok(years);
        }

        [HttpGet("months")]
        public async Task<IActionResult> GetMonths()
        {
            var months = await _payslipRepository.GetMonths();
            return Ok(months);
        }

        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _payslipRepository.GetEmployees();
            return Ok(employees);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            try
            {
                var data = await _payslipRepository.UploadPayslipData(file);
                return Ok(data);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save(List<string[]> data, [FromQuery] bool update = false)
        {
            try
            {
                await _payslipRepository.SavePayslipData(data, update);
                return Ok("Data saved successfully.");
            }
            catch (DataExistsException ex)
            {
                return Conflict(ex.Message); // Return 409 Conflict status
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("download/{year}/{month}/{empName}")]
        public async Task<IActionResult> DownloadPayslip(int year, string month, string empName)
        {
            // Retrieve the payslip data from the repository
            var payslip = await _payslipRepository.GetPayslip(year, month, empName);

            if (payslip == null)
            {
                return NotFound($"Payslip not found for {empName} in {month} {year}.");
            }

            // Load the HTML template (from a file or string)
            string htmlTemplate = System.IO.File.ReadAllText("Templates/PayslipTemplate.html");
            string totalPayInWords = ConvertNumberToWords(payslip.NetPayable);

            // Replace placeholders with actual data
            string htmlContent = htmlTemplate
                .Replace("{employee_name}", payslip.EmployeeName)
                .Replace("{EmpCode}", payslip.EmpCode)
                .Replace("{CTC}", payslip.CTC.ToString("C"))
                .Replace("{Month}", payslip.Month)
                .Replace("{Year}", payslip.Year.ToString())
                .Replace("{BasicPay}", payslip.EarnBasic.ToString("C"))
                .Replace("{HRA}", payslip.HRA.ToString("C"))
                .Replace("{MedicalAllowance}", payslip.MedicalAllowance.ToString("C"))
                .Replace("{ProvidentFund}", payslip.PF.ToString("C"))
                .Replace("{TotalDeductions}", payslip.TotalDeductions.ToString("C"))
                .Replace("{NetPayable}", payslip.NetPayable.ToString("C"))
                .Replace("{ProfessionalTax}", payslip.ProfessionalTax.ToString("C"))
                .Replace("{Designation}", payslip.Designation)
                .Replace("{Reimbursement}", payslip.Reimbursement.ToString("C"))
                .Replace("{AdvancePayment}", payslip.AdvancePayment.ToString("C"))
                .Replace("{designation}", payslip.Designation)
                .Replace("{OtherBasic}", payslip.OtherBasic.ToString("C"))
                .Replace("{TotalPay}", payslip.NetPayable.ToString("C"))
                .Replace("{TotalPayInWords}", totalPayInWords)
                .Replace("{datenow}", DateTime.Now.ToString("dd/MM/yyyy")); // Replace the pay date placeholder
            

            // Convert HTML to PDF
            using var stream = new MemoryStream();
            using (var pdfWriter = new PdfWriter(stream))
            {
                HtmlConverter.ConvertToPdf(htmlContent, pdfWriter);
            }

            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf", $"Payslip_{empName}_{month}_{year}.pdf");
        }

        [HttpGet("preview/{year}/{month}/{empName}")]
        public async Task<IActionResult> PreviewPayslip(int year, string month, string empName)
        {
            // Retrieve the payslip data from the repository
            var payslip = await _payslipRepository.GetPayslip(year, month, empName);

            if (payslip == null)
            {
                return NotFound($"Payslip not found for {empName} in {month} {year}.");
            }

            // Load the HTML template (from a file or string)
            string htmlTemplate = System.IO.File.ReadAllText("Templates/PayslipTemplate.html");
            string totalPayInWords = ConvertNumberToWords(payslip.NetPayable);

            // Replace placeholders with actual data
            string htmlContent = htmlTemplate
                .Replace("{employee_name}", payslip.EmployeeName)
                .Replace("{EmpCode}", payslip.EmpCode)
                .Replace("{CTC}", payslip.CTC.ToString("C"))
                .Replace("{Month}", payslip.Month)
                .Replace("{Year}", payslip.Year.ToString())
                .Replace("{BasicPay}", payslip.EarnBasic.ToString("C"))
                .Replace("{HRA}", payslip.HRA.ToString("C"))
                .Replace("{MedicalAllowance}", payslip.MedicalAllowance.ToString("C"))
                .Replace("{ProvidentFund}", payslip.PF.ToString("C"))
                .Replace("{TotalDeductions}", payslip.TotalDeductions.ToString("C"))
                .Replace("{NetPayable}", payslip.NetPayable.ToString("C"))
                .Replace("{ProfessionalTax}", payslip.ProfessionalTax.ToString("C"))
                .Replace("{Designation}", payslip.Designation)
                .Replace("{Reimbursement}", payslip.Reimbursement.ToString("C"))
                .Replace("{AdvancePayment}", payslip.AdvancePayment.ToString("C"))
                .Replace("{designation}", payslip.Designation)
                .Replace("{OtherBasic}", payslip.OtherBasic.ToString("C"))
                .Replace("{TotalPay}", payslip.NetPayable.ToString("C"))
                .Replace("{TotalPayInWords}", totalPayInWords)
                .Replace("{datenow}", DateTime.Now.ToString("dd/MM/yyyy")); // Replace the pay date placeholder;

            return Ok(htmlContent);
        }

        public string ConvertNumberToWords(decimal number)
        {
            // Separate the whole number part and the fractional part
            int wholeNumber = (int)Math.Floor(number);  // Get the integer part
            int fractionalPart = (int)((number - wholeNumber) * 100); // Get the fractional part (up to 2 decimal places)

            // Convert whole number part to words
            string wholeNumberInWords = wholeNumber.ToWords(CultureInfo.InvariantCulture);

            // Convert the fractional part to words
            string fractionalPartInWords = fractionalPart > 0 ? ConvertFractionalToWords(fractionalPart) : string.Empty;

            // Combine the whole number and fractional part in words
            return $"{wholeNumberInWords} . {fractionalPartInWords}".Trim();
        }

        // Helper method to convert fractional part (up to two decimal places) to words
        private string ConvertFractionalToWords(int fractionalPart)
        {
            // Create a number to words for the fractional part (1-99)
            return fractionalPart.ToWords(CultureInfo.InvariantCulture).ToLower();
        }
    }
}
