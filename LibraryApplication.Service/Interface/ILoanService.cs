using LibraryApplication.Domain.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryApplication.Service.Interface
{
    public interface ILoanService
    {
        Loan? GetById(Guid loanId);
        Loan? Update(Loan loan);
        Loan LoanBook(Guid bookId, string userId);        
        void ReturnBook(Guid loanId, string userId);      
        List<Loan> GetLoansByUser(string userId);
        List<Loan> GetLoansHistoryByUser(string userId);
        Loan? GetActiveLoanForBook(Guid bookId);
    }
}
