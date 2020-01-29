using System;
using System.Net.Http;
using NetsEncrypt.ACMEClient.Model;

namespace NetsEncrypt.ACMEClient.ACME
{
    public class ACMEException : Exception
    {
        public ACMEException(Problem problem, HttpResponseMessage response)
            : base($"{problem.Type}: {problem.Detail}")
        {
            Problem = problem;
            Response = response;
        }

        public Problem Problem { get; }

        public HttpResponseMessage Response { get; }
    }
}
