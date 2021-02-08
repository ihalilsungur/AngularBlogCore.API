using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngularBlogCore.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public IActionResult IsAuthenticated(AdminUser adminUser)
        {
            bool status = false;
            if (adminUser.Email == "sungur@gmail.com" && adminUser.Password == "12345")
            {
                status = true;
            }

            var result = new
            {
                status = status

            };
            return Ok(result);
        }
    }
}
