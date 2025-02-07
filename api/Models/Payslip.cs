namespace api.Models
{
    public class Payslip
    {
        public string Month { get; set; }
        public int Year { get; set; }
        public string EmpCode { get; set; }
        public string EmployeeName { get; set; }
        public string PAN { get; set; }
        public string BankAccount { get; set; }
        public string Designation { get; set; }
        public decimal CTC { get; set; }
        public decimal EarnBasic { get; set; }
        public decimal HRA { get; set; }
        public decimal MedicalAllowance { get; set; }
        public decimal OtherBasic { get; set; }
        public decimal PF { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal ESIC { get; set; }
        public decimal TDS { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal Difference { get; set; }
        public decimal Reimbursement { get; set; }
        public decimal AdvancePayment { get; set; }
        public decimal NetPayable { get; set; }
        public decimal YearlyCTC { get; set; }
    }

}
