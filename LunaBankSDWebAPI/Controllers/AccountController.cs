﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using SharpDevelopWebApi;
using SharpDevelopWebApi.Helpers.JWT;

namespace SharpDevelopWebApi.Controllers
{
    public class AccountController : ApiController
	{
        [HttpPost]
        [Route("TOKEN")]
        public IHttpActionResult GetToken(string email, string password)
        {
            if (UserAccount.Authenticate(email, password))
            {
                var data = new { Token = JwtManager.GenerateToken(email) };
                return Ok(data);
            }

            return BadRequest("Login failed");
        }

        [HttpGet]
        [Route("api/account/login")]
        public IHttpActionResult Login(string email, string password)
        {
            var success = UserAccount.Authenticate(email, password);
            if (success)
            {
                HttpContext.Current.Session.Add("currentUser", email);
                return Ok("Login successful");
            }
            else
                return BadRequest("Login failed");
        }

        [HttpGet]
        [Route("api/account/logout")]
        public IHttpActionResult Logout()
        {
            HttpContext.Current.Session.Clear();
            return Ok("Logout successful");
        }

        [HttpPost]
        [Route("api/account/register")]
        public IHttpActionResult Register(string email, string password)
        {
            var user = UserAccount.GetUserByEmail(email);
            if(user != null)
                return BadRequest("Account already exists");

            var userId = UserAccount.Create(email, password);
            if (userId != null)
                return Ok(new { UserId = userId, Message = "Account successfully created" });
            else
                return BadRequest("Account registration failed");
        }

        [HttpPost]
        [Route("api/account/changepassword")]
        [ApiAuthorize]
        public IHttpActionResult ChangePassword(string email, string newPassword, string currentPassword = "")
        {
            var currentUser = !string.IsNullOrEmpty(User.Identity.Name) ? User.Identity.Name : (string)HttpContext.Current.Session["currentUser"];
            var forceChangeIfAdmin = Array.IndexOf(UserAccount.GetUserRoles(currentUser), "admin") > -1;

            var success = UserAccount.ChangePassword(email, currentPassword, newPassword, forceChangeIfAdmin);
            if (success)
            {
                HttpContext.Current.Session.Clear();
                return Ok("Password successfully changed");
            }
            else
                return BadRequest("Password change failed");
        }
    }



}