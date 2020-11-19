using SlothEnterprise.ProductApplication.Products;

namespace SlothEnterprise.ProductApplication.Applications
{
    public interface ISellerApplication
    {
        IProduct Product { get; set; }

        ISellerCompanyData CompanyData { get; set; }
    }
}