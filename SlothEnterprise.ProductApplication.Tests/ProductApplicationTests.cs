﻿using System;
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using SlothEnterprise.External;
using SlothEnterprise.External.V1;
using SlothEnterprise.ProductApplication.Applications;
using SlothEnterprise.ProductApplication.Products;

namespace SlothEnterprise.ProductApplication.Tests
{
    [TestFixture]
    public class ProductApplicationTests
    {
        private readonly Fixture _fixture = new Fixture();
        private ISelectInvoiceService _selectInvoiceService;
        private IConfidentialInvoiceService _confidentialInvoiceService;
        private IBusinessLoansService _businessLoansService;
        private IProductApplicationService _productApplicationService;

        [OneTimeSetUp]
        public void Init()
        {
            _selectInvoiceService = A.Fake<ISelectInvoiceService>();
            _confidentialInvoiceService = A.Fake<IConfidentialInvoiceService>();
            _businessLoansService = A.Fake<IBusinessLoansService>();

            _productApplicationService = new ProductApplicationService(
                _selectInvoiceService,
                _confidentialInvoiceService,
                _businessLoansService);

            A.CallTo(() => _selectInvoiceService.SubmitApplicationFor(
                A<string>._,
                A<decimal>._,
                A<decimal>._)).Returns(1);

            A.CallTo(
                () => _confidentialInvoiceService.SubmitApplicationFor(
                    A<CompanyDataRequest>._,
                    A<decimal>._,
                    A<decimal>._,
                    A<decimal>._)).Returns(CreateSuccessApplicationResult());

            A.CallTo(
                    () => _businessLoansService.SubmitApplicationFor(
                        A<CompanyDataRequest>._,
                        A<LoansRequest>._))
                .Returns(CreateSuccessApplicationResult());
        }


        [Test]
        public void SubmitApplicationFor_ShouldCallSelectInvoiceService_WhenGetSelectiveInvoiceDiscountProduct()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = _fixture.Build<SelectiveInvoiceDiscount>().Create(),
                                        CompanyData = CreateCompanyData()
                                    };

            _productApplicationService.SubmitApplicationFor(sellerApplication);

            A.CallTo(
                    () => _selectInvoiceService.SubmitApplicationFor(
                        A<string>._,
                        A<decimal>._,
                        A<decimal>._))
                .MustHaveHappened();
        }

        [Test]
        public void
            SubmitApplicationFor_ShouldCallConfidentialInvoiceService_WhenGetConfidentialInvoiceDiscountProduct()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = _fixture.Build<ConfidentialInvoiceDiscount>().Create(),
                                        CompanyData = CreateCompanyData()
                                    };

            _productApplicationService.SubmitApplicationFor(sellerApplication);

            A.CallTo(
                    () => _confidentialInvoiceService.SubmitApplicationFor(
                        A<CompanyDataRequest>._,
                        A<decimal>._,
                        A<decimal>._,
                        A<decimal>._))
                .MustHaveHappened();
        }

        [Test]
        public void SubmitApplicationFor_ShouldCallBusinessLoansService_WhenGetBusinessLoansProduct()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = _fixture.Build<BusinessLoans>().Create(),
                                        CompanyData = CreateCompanyData()
                                    };

            _productApplicationService.SubmitApplicationFor(sellerApplication);

            A.CallTo(
                    () => _businessLoansService.SubmitApplicationFor(
                        A<CompanyDataRequest>._,
                        A<LoansRequest>._))
                .MustHaveHappened();
        }

        [Test]
        public void SubmitApplicationFor_ShouldThrowArgumentException_WhenGetNullOfCompanyData()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = _fixture.Build<BusinessLoans>().Create(),
                                        CompanyData = null
                                    };

            Assert.Throws(typeof(ArgumentException),
                () => _productApplicationService.SubmitApplicationFor(sellerApplication));
        }

        [Test]
        public void SubmitApplicationFor_ShouldThrowArgumentException_WhenGetNullOfProduct()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = _fixture.Build<BusinessLoans>().Create(),
                                        CompanyData = null
                                    };

            Assert.Throws(typeof(ArgumentException),
                () => _productApplicationService.SubmitApplicationFor(sellerApplication));
        }

        [Test]
        public void SubmitApplicationFor_ShouldThrowArgumentException_WhenGetProductTypeIsUnknown()
        {
            var sellerApplication = new SellerApplication
                                    {
                                        Product = A.Fake<IProduct>(),
                                        CompanyData = null
                                    };

            Assert.Throws(typeof(ArgumentException),
                () => _productApplicationService.SubmitApplicationFor(sellerApplication));
        }

        private IApplicationResult CreateSuccessApplicationResult()
        {
            var applicationResult = A.Fake<IApplicationResult>();
            applicationResult.Success = true;
            return applicationResult;
        }

        private ISellerCompanyData CreateCompanyData() => _fixture.Build<SellerCompanyData>().Create();
    }
}