using System;
using SlothEnterprise.External;
using SlothEnterprise.External.V1;
using SlothEnterprise.ProductApplication.Applications;
using SlothEnterprise.ProductApplication.Products;

namespace SlothEnterprise.ProductApplication
{
    internal class ProductApplicationService : IProductApplicationService
    {
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

            if (application.Product is SelectiveInvoiceDiscount sid)
            {
                return _selectInvoiceService.SubmitApplicationFor(
                    application.CompanyData.Number.ToString(),
                    sid.InvoiceAmount,
                    sid.AdvancePercentage);
            }

            var companyDataRequest = GetCompanyDataRequest(application.CompanyData);

            if (application.Product is ConfidentialInvoiceDiscount cid)
            {
                var result = _confidentialInvoiceWebService.SubmitApplicationFor(
                    companyDataRequest,
                    cid.TotalLedgerNetworth,
                    cid.AdvancePercentage,
                    cid.VatRate);

                return result.Success ? result.ApplicationId ?? -1 : -1;
            }

            if (application.Product is BusinessLoans loans)
            {
                var result = _businessLoansService.SubmitApplicationFor(
                    companyDataRequest,
                    new LoansRequest
                    {
                        InterestRatePerAnnum = loans.InterestRatePerAnnum,
                        LoanAmount = loans.LoanAmount
                    });
                return result.Success ? result.ApplicationId ?? -1 : -1;
            }

            throw new InvalidOperationException();
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
    }
}