using LetsEncryptClient.Model;
using System;
using System.Net.Http;

namespace LetsEncryptClient
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
