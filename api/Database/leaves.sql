CREATE TABLE IF NOT EXISTS "Leaves" (
    "EmpCode" TEXT NOT NULL,                      -- Employee code (Foreign Key from Payslip)
    "EmployeeName" TEXT NOT NULL,                  -- Employee name (Foreign Key from Payslip, though better use EmpCode alone)
    "LeaveYear" INTEGER NOT NULL,                  -- The year of the leave record
    "LeaveFromJanuary" INTEGER NOT NULL,           -- Leave balance at the start of January (if applicable)
    "Jan" INTEGER NOT NULL, "Feb" INTEGER NOT NULL, "Mar" INTEGER NOT NULL,  -- Monthly leave data
    "Apr" INTEGER NOT NULL, "May" INTEGER NOT NULL, "Jun" INTEGER NOT NULL, 
    "Jul" INTEGER NOT NULL, "Aug" INTEGER NOT NULL, "Sep" INTEGER NOT NULL, 
    "Oct" INTEGER NOT NULL, "Nov" INTEGER NOT NULL, "Dec" INTEGER NOT NULL,
    "BalanceLeaves" INTEGER NOT NULL,             -- Remaining leaves after accounting for all months
    FOREIGN KEY ("EmpCode") REFERENCES "Payslip"("EmpCode"),  -- Foreign key for EmpCode
    FOREIGN KEY ("EmployeeName") REFERENCES "Payslip"("EmployeeName")  -- Not recommended as explained
);
