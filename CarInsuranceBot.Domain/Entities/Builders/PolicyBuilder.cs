using System;

namespace CarInsuranceBot.Domain.Entities.Builders
{
    public class PolicyBuilder
    {
        private readonly Policy _policy = new();

        public PolicyBuilder WithUserId(Guid userId)
        {
            _policy.UserId = userId;
            return this;
        }

        public PolicyBuilder WithPolicyNumber(string policyNumber)
        {
            _policy.PolicyNumber = policyNumber;
            return this;
        }

        public PolicyBuilder WithStatus(PolicyStatus status)
        {
            _policy.Status = status;
            return this;
        }

        public PolicyBuilder WithPdfPath(string pdfPath)
        {
            _policy.PdfPath = pdfPath;
            return this;
        }

        public PolicyBuilder WithIssuedUtc(DateTime issuedUtc)
        {
            _policy.IssuedUtc = issuedUtc;
            return this;
        }

        public PolicyBuilder WithExpiresUtc(DateTime expiresUtc)
        {
            _policy.ExpiresUtc = expiresUtc;
            return this;
        }

        public PolicyBuilder WithUser(User user)
        {
            _policy.User = user;
            return this;
        }

        public Policy Build() => _policy;
    }
} 