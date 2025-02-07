using api.Models;


namespace api.Repositories
{
    public interface IPayslipRepository
    {
        Task<List<string[]>> UploadPayslipData(IFormFile file);
        Task<bool> SavePayslipData(List<string[]> data, bool updateExisting = false);
        Task<Payslip?> GetPayslipByEmpCode(string empCode);
        Task<List<int>> GetYears();
        Task<List<string>> GetMonths();
        Task<List<string>> GetEmployees();
        Task<Payslip?> GetPayslip(int year, string month, string empName);
    }
}
