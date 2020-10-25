using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CommonData.Messages;
using CommonData.Models;
using Library.Web.Helper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly IRequestClient<Message<User>> _userClient;
        private readonly IRequestClient<MessageResponse<User>> _userResponseClient;
        private readonly IRequestClient<MessageResponse<UserUpdate>> _userUpdateResponseClient;

        private const string SessionStatus = "_Status"; // 0 - niezalogowany, 1 - zalogowany, 2 - admin
        private const string SessionName = "_Name";
        private const string SessionUserEmail = "_Email";
        private const string SessionUserId = "_UserID";


        public UsersController(IRequestClient<Message<User>> userClient,
            IRequestClient<MessageResponse<User>> userResponseClient,
            IRequestClient<MessageResponse<UserUpdate>> userUpdateResponseClient)
        {
            _userClient = userClient;
            _userResponseClient = userResponseClient;
            _userUpdateResponseClient = userUpdateResponseClient;
        }

        public IActionResult Login()
        {
            HttpContext.Session.SetInt32(SessionStatus, 0);
            HttpContext.Session.SetString(SessionName, "");
            //HttpContext.Session.SetString(SessionUserEmail, "");
            HttpContext.Session.SetString(SessionUserId, "");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            var userMessage = new List<User>()
            {
                new User
                {
                    Login = user.Login,
                    Password = user.Password
                }
            };

            try
            {
                var response = await _userClient.GetResponse<Message<User>>(
                    new
                    {
                        Action = Variables.ActionLogin,
                        MessageContent = userMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    HttpContext.Session.SetString(SessionName, response.Message.MessageContent.FirstOrDefault().FullName);
                    HttpContext.Session.SetString(SessionUserEmail, response.Message.MessageContent.FirstOrDefault().Email);
                    HttpContext.Session.SetString(SessionUserId, response.Message.MessageContent.FirstOrDefault().Id);

                    if (response.Message.MessageContent.FirstOrDefault().Login == StaticVariables.AdminId)
                    {
                        HttpContext.Session.SetInt32(SessionStatus, 2);
                    }
                    else
                    {
                        HttpContext.Session.SetInt32(SessionStatus, 1);
                    }

                    return RedirectToAction("Rented", "Books");
                }

                else if (response.Message.StatusCode == 400)
                {
                    ViewBag.ErrorMsg = "Incorrect card id or password.";
                }
                return View();
            }
            catch (RequestTimeoutException timeException)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(timeException.Message);
                ViewBag.ErrorMsg = "Your request timed out.";
                return View();
            }
            catch (Exception e)
            {
                HttpContext.Session.Clear();
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            var userMessage = new List<User>()
            {
                new User
                {
                    Email = user.Email,
                    Login = user.Login,
                    Password = user.Password,
                    FullName = user.FullName
                }
            };

            try
            {
                var response = await _userResponseClient.GetResponse<MessageResponse<User>>(
                    new
                    {
                        Action = Variables.ActionRegister,
                        MessageContent = userMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    return RedirectToAction("Login");
                }

                else if (response.Message.StatusCode == 400)
                {
                    ViewBag.ErrorMsg = "User with this card id already exists!";
                    return View();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                ViewBag.ErrorMsg = "Unknown error occured.";
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Update()
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);
            ViewBag.fullName = HttpContext.Session.GetString(SessionName);
            ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);

            var userMessage = new List<User>()
            {
                new User
                {
                    Id = userId
                }
            };

            try
            {
                var response = await _userClient.GetResponse<Message<User>>(
                    new
                    {
                        Action = Variables.ActionUpdate,
                        MessageContent = userMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    var userEmail = response.Message.MessageContent.FirstOrDefault().Email;
                    var userUpdate = new UserUpdate {OldEmail = userEmail};

                    return View(userUpdate);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return RedirectToAction("ListAvailable", "Books");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(UserUpdate userUpdate)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            var userMessage = new List<UserUpdate>()
            {
                new UserUpdate
                {
                    OldPassword = userUpdate.OldPassword,
                    NewPassword = userUpdate.NewPassword,
                    RepeatNewPassword = userUpdate.RepeatNewPassword
                }
            };

            try
            {
                var response = await _userUpdateResponseClient.GetResponse<MessageResponse<UserUpdate>>(
                    new
                    {
                        Action = Variables.ActionUpdatePassword,
                        UserId = userId,
                        MessageContent = userMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    ViewBag.ErrorMsg = "Password changed.";
                    HttpContext.Session.SetInt32(SessionStatus, 0);
                    HttpContext.Session.SetString(SessionName, "");
                    HttpContext.Session.SetString(SessionUserId, "");
                    return View("Login");
                }

                else if (response.Message.StatusCode == 400)
                {
                    ViewBag.ErrorMsg = "Password update error.";
                    ViewBag.fullName = HttpContext.Session.GetString(SessionName);
                    ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
                    ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return View("Update", userUpdate);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmail(UserUpdate userUpdate)
        {
            var userId = HttpContext.Session.GetString(SessionUserId);
            if (userId == "")
            {
                return RedirectToAction("Login", "Users");
            }

            var userMessage = new List<UserUpdate>()
            {
                new UserUpdate
                {
                    OldEmail = userUpdate.OldEmail,
                    NewEmail = userUpdate.NewEmail,
                    OldPasswordEmail = userUpdate.OldPasswordEmail
                }
            };

            try
            {
                var response = await _userUpdateResponseClient.GetResponse<MessageResponse<UserUpdate>>(
                    new
                    {
                        Action = Variables.ActionUpdateEmail,
                        UserId = userId,
                        MessageContent = userMessage
                    });

                if (response.Message.StatusCode == 200)
                {
                    // powodzenie
                    ViewBag.ErrorMsg = "Email changed.";
                    HttpContext.Session.SetInt32(SessionStatus, 0);
                    HttpContext.Session.SetString(SessionName, "");
                    HttpContext.Session.SetString(SessionUserId, "");
                    return View("Login");
                }

                else if (response.Message.StatusCode == 400)
                {
                    // blad
                    ViewBag.ErrorMsg = "Email update error.";
                    ViewBag.fullName = HttpContext.Session.GetString(SessionName);
                    ViewBag.Email = HttpContext.Session.GetString(SessionUserEmail);
                    ViewBag.status = HttpContext.Session.GetInt32(SessionStatus);
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return View("Update", userUpdate);
        }
    }
}