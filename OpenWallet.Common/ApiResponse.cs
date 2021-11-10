using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{

    public class ApiResponse<T> where T: class, new()
    {
        public bool success { get; internal set; }
        public T Payload { get; set; }

        public ApiResponse(T response)
        {
            Payload = response;
            success = true;
        }

        public ApiResponse()
        {
            success = false;
            Payload = new T();
        }
    }

}
