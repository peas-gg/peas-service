using System;
using Microsoft.AspNetCore.Mvc;
using PEAS.Entities.Authentication;

namespace PEAS.Controllers
{
    [Controller]
    public abstract class BaseController : ControllerBase
    {
        // returns the current authenticated account (null if not logged in)
        public Account? Account => (Account?)HttpContext.Items["Account"];
    }
}