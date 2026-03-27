using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DirectoryService.Presenters
{
    [ApiController]
    [Route("[controller]")]
    public class DepartmentController : ControllerBase
    {
        [HttpGet]
        public void Test()
        {

        }
    }
}
