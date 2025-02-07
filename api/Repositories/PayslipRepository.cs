using api.Models;
using Dapper;
using Microsoft.Data.Sqlite;
using OfficeOpenXml;


namespace api.Repositories
{

    public class PayslipRepository : IPayslipRepository
    {
        private readonly string _connectionString;
        private readonly List<string> _expectedColumns = new List<string>
            {
                "Month", "Year", "EmpCode", "EmployeeName", "PAN", "BankAccount", "Designation",
                "CTC", "EarnBasic", "HRA", "MedicalAllowance", "OtherBasic", "PF", "ProfessionalTax",
                "ESIC", "TDS", "TotalDeductions", "Difference", "Reimburshment", "AdvancePayment",
                "NetPayable", "YearlyCTC"
            };

        public PayslipRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Database connection string is missing.");
        }

        public async Task<List<int>> GetYears()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var sql = "SELECT DISTINCT Year FROM Payslip ORDER BY Year";
                var years = await connection.QueryAsync<int>(sql);
                return years.ToList();
            }
        }

        public async Task<List<string>> GetMonths()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var sql = "SELECT DISTINCT Month FROM Payslip ORDER BY Month";
                var months = await connection.QueryAsync<string>(sql);
                return months.ToList();
            }
        }

        public async Task<List<string>> GetEmployees()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var sql = "SELECT DISTINCT EmployeeName FROM Payslip ORDER BY EmployeeName";
                var employees = await connection.QueryAsync<string>(sql);
                return employees.ToList();
            }
        }

        public async Task<Payslip?> GetPayslip(int year, string month, string empName)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var sql = "SELECT * FROM Payslip WHERE Year = @Year AND Month = @Month AND EmployeeName = @EmployeeName";
                var parameters = new { Year = year, Month = month, EmployeeName = empName };
                var payslip = await connection.QueryFirstOrDefaultAsync<Payslip>(sql, parameters);
                return payslip;
            }
        }

        public async Task<List<string[]>> UploadPayslipData(IFormFile file)
        {
            List<string[]> data = new List<string[]>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    // Validate columns
                    var headers = new List<string>();
                    for (int col = 1; col <= colCount; col++)
                    {
                        var header = worksheet.Cells[1, col]?.Text?.Trim();
                        headers.Add(header ?? string.Empty);
                    }

                    if (!headers.SequenceEqual(_expectedColumns))
                    {
                        throw new InvalidDataException("The uploaded file does not have the correct columns.");
                    }

                    for (int row = 2; row <= rowCount; row++) // Assuming row 1 is header
                    {
                        List<string> rowData = new List<string>();

                        for (int col = 1; col <= colCount; col++)
                        {
                            var cellValue = worksheet.Cells[row, col]?.Text?.Trim();
                            rowData.Add(cellValue ?? string.Empty);
                        }

                        // Ensure the row has enough columns before adding it to the data list
                        if (rowData.Count != _expectedColumns.Count)
                        {
                            throw new InvalidDataException($"Row {row} does not have the correct number of columns.");
                        }

                        // Log to verify if all data is being read properly
                        Console.WriteLine($"Row {row}: {string.Join(", ", rowData)}");

                        data.Add(rowData.ToArray());
                    }
                }
            }

            return data;
        }

        public async Task<bool> CheckIfUserExists(string empCode, string month, int year)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                var checkSql = "SELECT COUNT(1) FROM Payslip WHERE EmpCode = @EmpCode AND Month = @Month AND Year = @Year";
                var parameters = new { EmpCode = empCode, Month = month, Year = year };
                var exists = await connection.ExecuteScalarAsync<int>(checkSql, parameters) > 0;
                return exists;
            }
        }

        public async Task<bool> SavePayslipData(List<string[]> payslipData, bool updateExisting = false)
        {
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        foreach (var row in payslipData)
                        {
                            // Ensure the row has enough columns before accessing them
                            if (row.Length < 22)
                            {
                                Console.WriteLine("Skipping row due to missing columns: " + string.Join(", ", row));
                                continue; // Skip incomplete records
                            }

                            var parameters = new
                            {
                                Month = row[0],
                                Year = int.TryParse(row[1], out int year) ? year : 0,
                                EmpCode = row[2],
                                EmployeeName = row[3],
                                PAN = row[4],
                                BankAccount = row[5],
                                Designation = row[6],
                                CTC = double.TryParse(row[7], out double ctc) ? ctc : 0,
                                EarnBasic = double.TryParse(row[8], out double earnBasic) ? earnBasic : 0,
                                HRA = double.TryParse(row[9], out double hra) ? hra : 0,
                                MedicalAllowance = double.TryParse(row[10], out double medicalAllowance) ? medicalAllowance : 0,
                                OtherBasic = double.TryParse(row[11], out double otherBasic) ? otherBasic : 0,
                                PF = double.TryParse(row[12], out double pf) ? pf : 0,
                                ProfessionalTax = double.TryParse(row[13], out double professionalTax) ? professionalTax : 0,
                                ESIC = double.TryParse(row[14], out double esic) ? esic : 0,
                                TDS = double.TryParse(row[15], out double tds) ? tds : 0,
                                TotalDeductions = double.TryParse(row[16], out double totalDeductions) ? totalDeductions : 0,
                                Difference = double.TryParse(row[17], out double difference) ? difference : 0,
                                Reimburshment = double.TryParse(row[18], out double reimburshment) ? reimburshment : 0,
                                AdvancePayment = double.TryParse(row[19], out double advancePayment) ? advancePayment : 0,
                                NetPayable = double.TryParse(row[20], out double netPayable) ? netPayable : 0,
                                YearlyCTC = double.TryParse(row[21], out double yearlyCTC) ? yearlyCTC : 0
                            };

                            var exists = await CheckIfUserExists(parameters.EmpCode, parameters.Month, parameters.Year);

                            if (exists)
                            {
                                if (updateExisting)
                                {
                                    var updateSql = @"
                                        UPDATE Payslip SET
                                            EmployeeName = @EmployeeName, PAN = @PAN, BankAccount = @BankAccount, Designation = @Designation, 
                                            CTC = @CTC, EarnBasic = @EarnBasic, HRA = @HRA, MedicalAllowance = @MedicalAllowance, 
                                            OtherBasic = @OtherBasic, PF = @PF, ProfessionalTax = @ProfessionalTax, ESIC = @ESIC, 
                                            TDS = @TDS, TotalDeductions = @TotalDeductions, Difference = @Difference, 
                                            Reimburshment = @Reimburshment, AdvancePayment = @AdvancePayment, NetPayable = @NetPayable, 
                                            YearlyCTC = @YearlyCTC
                                        WHERE EmpCode = @EmpCode AND Month = @Month AND Year = @Year";
                                    await connection.ExecuteAsync(updateSql, parameters, transaction);
                                }
                                else
                                {
                                    throw new DataExistsException("Data already exists and has been updated.");
                                }
                            }
                            else
                            {
                                var insertSql = @"
                                    INSERT INTO Payslip (
                                        Month, Year, EmpCode, EmployeeName, PAN, BankAccount, Designation, 
                                        CTC, EarnBasic, HRA, MedicalAllowance, OtherBasic, PF, ProfessionalTax, 
                                        ESIC, TDS, TotalDeductions, Difference, Reimburshment, AdvancePayment, 
                                        NetPayable, YearlyCTC
                                    ) 
                                    VALUES (
                                        @Month, @Year, @EmpCode, @EmployeeName, @PAN, @BankAccount, @Designation, 
                                        @CTC, @EarnBasic, @HRA, @MedicalAllowance, @OtherBasic, @PF, @ProfessionalTax, 
                                        @ESIC, @TDS, @TotalDeductions, @Difference, @Reimburshment, @AdvancePayment, 
                                        @NetPayable, @YearlyCTC
                                    );";
                                await connection.ExecuteAsync(insertSql, parameters, transaction);
                            }
                        }

                        transaction.Commit();
                    }
                }

                return true;
            }
            catch (DataExistsException)
            {
                throw; // Rethrow custom exception to be handled by the controller
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database Save Error: {ex.Message}");
                throw new Exception("Error saving data.");
            }
        }

        public async Task<Payslip?> GetPayslipByEmpCode(string empCode)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string query = "SELECT * FROM Payslip WHERE EmpCode = @EmpCode";
                return await connection.QueryFirstOrDefaultAsync<Payslip>(query, new { EmpCode = empCode });
            }
        }

    }

    public class DataExistsException : Exception
    {
        public DataExistsException(string message) : base(message) { }
    }
}


