﻿using System.Net;

namespace CQ.AuthProvider.WebApi.Filters
{
    internal sealed record class ExceptionsOfOrigin
    {
        public readonly IDictionary<Type, ExceptionResponse> Exceptions = new Dictionary<Type, ExceptionResponse>();

        public ExceptionsOfOrigin AddException<TException>(
            string code,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null) 
            where TException : Exception
        {
                if (this.Exceptions.ContainsKey(typeof(TException))) return this;

                this.Exceptions.Add(
                    typeof(TException),
                    new DinamicExceptionResponse<TException>(
                        code,
                        statusCode,
                        messageFunction,
                        logMessageFunction));

            return this;
        }

        public ExceptionsOfOrigin AddException<TException>(
            string code,
            HttpStatusCode statusCode,
            string message,
            string? logMessage = null)
            where TException : Exception
        {
            if (this.Exceptions.ContainsKey(typeof(TException))) return this;

            this.Exceptions.Add(
                typeof(TException),
                new ExceptionResponse(
                    code,
                    statusCode,
                    message,
                    logMessage));

            return this;
        }
    }
}
