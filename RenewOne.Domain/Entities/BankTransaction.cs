using RenewOne.Domain.Common;

namespace RenewOne.Domain.Entities
{
    public class BankTransaction: BaseEntity
    {
        public Guid BankAccountId { get; set; }

        public string TransactionType { get; set; } // Credit / Debit

        public decimal Amount { get; set; }

        public string Category { get; set; }

        public string Description { get; set; }

        public Guid? PolicyPaymentId { get; set; }

        public Guid? SalaryId { get; set; }

        public Guid? VendorId { get; set; }

        public Guid? UserId { get; set; }

        public DateTime TransactionDate { get; set; }

        public virtual BankAccount BankAccount { get; set; }
    }
}
