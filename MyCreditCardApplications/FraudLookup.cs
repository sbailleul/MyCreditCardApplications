namespace CreditCardApplications
{
    public class FraudLookup
    {
        public bool IsFraudRisk(CreditCardApplication app)
        {
            return CheckApplication(app);
        }

        protected virtual bool CheckApplication(CreditCardApplication app)
        {
            return app.LastName == "Smith";

        }
    }
}