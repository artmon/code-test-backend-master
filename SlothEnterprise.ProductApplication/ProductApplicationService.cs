using System;
using System.Linq;
using SlothEnterprise.External;
using SlothEnterprise.External.V1;
using SlothEnterprise.ProductApplication.Applications;
using SlothEnterprise.ProductApplication.Products;

namespace SlothEnterprise.ProductApplication
{
    internal class ProductApplicationService : IProductApplicationService
    {
        private static readonly int codeOfWrongAnswerFromService = -1;
        private readonly ISelectInvoiceService _selectInvoiceService;
        private readonly IConfidentialInvoiceService _confidentialInvoiceWebService;
        private readonly IBusinessLoansService _businessLoansService;

        public ProductApplicationService(
            ISelectInvoiceService selectInvoiceService,
            IConfidentialInvoiceService confidentialInvoiceWebService,
            IBusinessLoansService businessLoansService)
        {
            _selectInvoiceService = selectInvoiceService;
            _confidentialInvoiceWebService = confidentialInvoiceWebService;
            _businessLoansService = businessLoansService;
        }

        public int SubmitApplicationFor(ISellerApplication application)
        {
            if (application?.CompanyData == null)
                throw new ArgumentException(nameof(application.CompanyData));

            if (application.Product == null)
                throw new ArgumentException(nameof(application.Product));

            try
            {
                switch (application.Product)
                {
                    case SelectiveInvoiceDiscount sid:
                        return CallSelectInvoiceService(application.CompanyData.Number, sid);
                    case ConfidentialInvoiceDiscount cid:
                        return CallConfidentialInvoiceWebService(application.CompanyData, cid);
                    case BusinessLoans loans:
                        return CallBusinessLoansService(application.CompanyData, loans);
                    default:
                        throw new ArgumentException(nameof(application.Product));
                }
            }
            catch (Exception e)
            {
                //TODO Log exception
                throw;
            }
        }

        private int CallSelectInvoiceService(int companyNumber, SelectiveInvoiceDiscount sid)
        {
            return _selectInvoiceService.SubmitApplicationFor(
                companyNumber.ToString(),
                sid.InvoiceAmount,
                sid.AdvancePercentage);
        }

        private int CallBusinessLoansService(ISellerCompanyData companyData, BusinessLoans loans)
        {
            var companyDataRequest = GetCompanyDataRequest(companyData);

            var result = _businessLoansService.SubmitApplicationFor(
                companyDataRequest,
                new LoansRequest
                {
                    InterestRatePerAnnum = loans.InterestRatePerAnnum,
                    LoanAmount = loans.LoanAmount
                });

            return ProcessApplicationResult(result);
        }

        private int CallConfidentialInvoiceWebService(ISellerCompanyData companyData, ConfidentialInvoiceDiscount cid)
        {
            var companyDataRequest = GetCompanyDataRequest(companyData);

            var result = _confidentialInvoiceWebService.SubmitApplicationFor(
                companyDataRequest,
                cid.TotalLedgerNetworth,
                cid.AdvancePercentage,
                cid.VatRate);

            return ProcessApplicationResult(result);
        }

        private static CompanyDataRequest GetCompanyDataRequest(ISellerCompanyData companyData)
        {
            return new CompanyDataRequest
                   {
                       CompanyFounded = companyData.Founded,
                       CompanyNumber = companyData.Number,
                       CompanyName = companyData.Name,
                       DirectorName = companyData.DirectorName
                   };
        }

        private int ProcessApplicationResult(IApplicationResult result)
        {
            return CheckResult(result) && result.ApplicationId.HasValue
                ? result.ApplicationId.Value
                : codeOfWrongAnswerFromService;
        }

        private bool CheckResult(IApplicationResult result)
        {
            if (result == null)
                return false;

            if (result.Errors?.Any() == true)
            {
                //TODO log errors
            }

            return result.Success;
        }
    }
}