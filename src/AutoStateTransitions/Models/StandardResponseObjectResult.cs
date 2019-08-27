using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Models
{
    public class StandardResponseObjectResult : ObjectResult
    {
        public StandardResponseObjectResult(object value, int statusCode) : base(value)
        {
            StatusCode = statusCode;
        }
    }
}
