using System;

namespace SuperMarket.Service
{
    class BusinessException : Exception
    {
        public BusinessException(string message) :
            base(message)
        {
        }
    }
}