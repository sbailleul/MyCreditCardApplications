using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using CreditCardApplications;
using Moq;
using Moq.Protected;
using Xunit;
using Range = Moq.Range;

namespace MyCreditCardApplications.Test
{
    public class CreditCardApplicationEvaluatorShould
    {
        private Mock<IFrequentFlyerNumberValidator> _mockValidator;
        private CreditCardApplicationEvaluator _sut;

        public CreditCardApplicationEvaluatorShould()
        {
            _mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            _mockValidator.SetupAllProperties();
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            _mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            _sut = new CreditCardApplicationEvaluator(_mockValidator.Object);
        }
        [Fact]
        public void AcceptHighIncomeApplications()
        {
            var application = new CreditCardApplication(){GrossAnnualIncome = 100_000};
            var decision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        public void DeclineLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            // var isValid = true;
            // mockValidator
            //     .Setup(x
            //         => x.IsValid(It.IsAny<string>(), out isValid));
            // mockValidator
            //     .Setup(x 
            //         => x.IsValid(It.Is<string>(number 
            //             => number.StartsWith('x'))))
            //     .Returns(true);
            // mockValidator
            //     .Setup(x 
            //         => x.IsValid(It.IsIn("x","y","z")))
            //     .Returns(true);
            // mockValidator
            //     .Setup(x 
            //         => x.IsValid(It.IsInRange("a","z",Range.Inclusive)))
            //     .Returns(true);
            mockValidator
                .Setup(x 
                    => x.IsValid(It.IsRegex("[a-z]+",RegexOptions.None)))
                .Returns(true);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var application = new CreditCardApplication{GrossAnnualIncome = 19_999, Age = 42,FrequentFlyerNumber = "x"};
            var decision = sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        public void ReferYoungApplications()
        {
            var application = new CreditCardApplication{Age = 19};
            var decision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void ReferInvalidFrequentFlyerApplications()
        {
            _mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);
            var app = new CreditCardApplication();
            var decision = _sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);

        }

        [Fact]
        public void ReferWhenLicenceKeyExpired()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator
                .Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true);
            // var mockLicenseData = new Mock<ILicenseData>();
            // mockLicenseData
            //     .Setup(x => x.LicenseKey)
            //     .Returns("EXPIRED"); 
            // var mockServiceInfo = new Mock<IServiceInformation>();
            // mockServiceInfo
            //     .Setup(x => x.License)
            //      .Returns(mockLicenseData.Object);
            // mockValidator
            //     .Setup(x => x.ServiceInformation).Returns(mockServiceInfo.Object);
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("EXPIRED");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication{Age = 42};
            var decision = sut.Evaluate(app); 
            
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void UseDetailedLookupForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.SetupAllProperties();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            // mockValidator.SetupProperty(x => x.ValidationMode);
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){Age = 30};

            var decision = sut.Evaluate(app);
            Assert.Equal(ValidationMode.Detailed, mockValidator.Object.ValidationMode);
        }

        [Fact]
        public void ValidateFrequentlyFlyerNumberForLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication{FrequentFlyerNumber = "q"};
            sut.Evaluate(app);
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Once);
        }
        
        // [Fact]
        // public void ValidateFrequentFlyerNumberForLowIncomeApplications_CustomMessage()
        // {
        //     var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
        //     mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
        //     var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
        //     var app = new CreditCardApplication();
        //     sut.Evaluate(app);
        //     mockValidator.Verify(x => x.IsValid(It.IsNotNull<string>()), "Frequent flyer number passed should not be null");
        // }
        //
        [Fact]
        public void ValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){GrossAnnualIncome = 100_000};
            sut.Evaluate(app);
            mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){GrossAnnualIncome = 99_000};
            sut.Evaluate(app);
            mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey);
        }
        
        [Fact]
        public void SetDetailedLookupForOlderApplications()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){Age = 30};
            sut.Evaluate(app);
            mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once);
        }

        [Fact]
        public void ReferWhenFrequentFlyerValidationError()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws(new Exception("Custom message"));
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){Age = 42};
            var decision = sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        public void IncrementLookupCount()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                .Returns(true)
                .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty );
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){FrequentFlyerNumber = "x", Age = 25};
            sut.Evaluate(app);
            // mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);
            Assert.Equal(1,sut.ValidatorLookupCount);

        }
        
        
        [Fact]
        public void ReferInvalidFrequentFlyerApplications_Sequence()
        {
            var mockValidator = new Mock<IFrequentFlyerNumberValidator>();
            mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>())).Returns(false).Returns(true);
            var sut = new CreditCardApplicationEvaluator(mockValidator.Object);
            var app = new CreditCardApplication(){Age = 25};
            var firstDecision = sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);
            var secondDecision = sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        public void ReferFraudRisk()
        {
            var mockFraudLookup = new Mock<FraudLookup>();
            
            // mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>())).Returns(true);
            mockFraudLookup.Protected().Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>()).Returns(true);
            _sut = new CreditCardApplicationEvaluator(_mockValidator.Object, mockFraudLookup.Object);
            var app = new CreditCardApplication();
            var decision = _sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }
        
        [Fact]
        public void LinqToMocks()
        {
            var mockValidator = Mock.Of<IFrequentFlyerNumberValidator>(
                validator =>
                    validator.ServiceInformation.License.LicenseKey == "OK" && validator.IsValid(It.IsAny<string>()) == true
            );
            var sut = new CreditCardApplicationEvaluator(mockValidator);
            var app = new CreditCardApplication(){Age = 25};
            var decision = sut.Evaluate(app);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }
        
        string GetLicenceKeyExpiryString()
        {
            return "EXPIRED";
        }
        
    }
}