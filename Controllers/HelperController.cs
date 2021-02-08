using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using AngularBlogCore.API.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class HelperController : ControllerBase
    {
        public IActionResult SendContactEmail(Contact contact)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("smtp.uygarmedikal.com", 587);
                mailMessage.From = new MailAddress("ibrahimsungur@uygarmedikal.com");
                mailMessage.To.Add(contact.Email);
                mailMessage.Subject = contact.Subject;
                mailMessage.Body = contact.Message;
                mailMessage.IsBodyHtml = true;
                // smtpClient.Port = 587;
                //smtpClient.EnableSsl = true;
                smtpClient.Credentials = new System.Net.NetworkCredential("ibrahimsungur@uygarmedikal.com", "Sungur63");
                smtpClient.Send(mailMessage);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
