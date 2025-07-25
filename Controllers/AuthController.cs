using System;
using System.Data;
using Dapper;
using Music.Data;
using Music.Dtos;
using Music.Helpers;
using Music.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Music.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;
        private readonly ReusableSql _reusableSql;
        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
            _reusableSql = new ReusableSql(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserForRegistrationDto userForRegistration)
        {
            // Add logging to see what's being received
            Console.WriteLine($"Register called with email: {userForRegistration?.Email}");

            try
            {
                // Validate input
                if (userForRegistration == null)
                {
                    Console.WriteLine("userForRegistration is null");
                    return BadRequest(new { success = false, error = "Invalid request data" });
                }

                if (string.IsNullOrEmpty(userForRegistration.Email))
                {
                    return BadRequest(new { success = false, error = "Email is required" });
                }

                if (userForRegistration.Password != userForRegistration.PasswordConfirm)
                {
                    return BadRequest(new { success = false, error = "Passwords do not match" });
                }

                string sqlCheckUserExists = @"SELECT Email FROM dbo.Auth WHERE Email = '" +
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() > 0)
                {
                    return Conflict(new
                    {
                        success = false,
                        error = "Account already exists",
                        message = "An account with this email address already exists. Please try logging in instead.",
                        code = "EMAIL_EXISTS"
                    });
                }

                UserForLoginDto userForSetPassword = new UserForLoginDto()
                {
                    Email = userForRegistration.Email,
                    Password = userForRegistration.Password
                };

                if (!_authHelper.SetPassword(userForSetPassword))
                {
                    return BadRequest(new { success = false, error = "Failed to register user" });
                }

                User user = new User()
                {
                    FirstName = userForRegistration.FirstName,
                    LastName = userForRegistration.LastName,
                    Email = userForRegistration.Email,
                    Active = true,
                };

                if (!_reusableSql.UpsertUser(user))
                {
                    return BadRequest(new { success = false, error = "Failed to add user to database" });
                }

                // Auto-login after successful registration
                string userIdSql = @"SELECT UserId FROM dbo.Users WHERE Email = '" + userForRegistration.Email + "'";
                int userId = _dapper.LoadDataSingle<int>(userIdSql);
                string token = _authHelper.CreateToken(userId);

                Console.WriteLine("Registration successful");
                return Ok(new
                {
                    success = true,
                    message = "Account created successfully!",
                    code = "ACCOUNT_CREATED",
                    token = token
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                return StatusCode(500, new { success = false, error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPut("ResetPassword")]
        public IActionResult ResetPassword(UserForLoginDto userForResetPassword)
        {
            if (_authHelper.SetPassword(userForResetPassword))
            {
                return Ok();
            }
            throw new Exception("Failed to update password");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            try
            {
                string sqlForHashAndSalt = @"EXEC dbo.spLoginConfirmation_Get 
                @Email = @EmailParam";

                DynamicParameters sqlParameters = new DynamicParameters();
                sqlParameters.Add("@EmailParam", userForLogin.Email, DbType.String);

                UserForLoginConfirmationDto userForConfirmation = _dapper
                    .LoadDataSingleWithParameters<UserForLoginConfirmationDto>(sqlForHashAndSalt, sqlParameters);

                byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                    {
                        return StatusCode(401, new { success = false, error = "Incorrect password!" });
                    }
                }

                string userIdSql = @"SELECT UserId FROM dbo.Users WHERE Email = '" + userForLogin.Email + "'";
                int userId = _dapper.LoadDataSingle<int>(userIdSql);

                return Ok(new
                {
                    success = true,
                    token = _authHelper.CreateToken(userId),
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpGet("RefreshToken")]

        public IActionResult RefreshToken()
        {
            string userId = User.FindFirst("userId")?.Value + "";

            string userIdSql = @"SELECT UserId FROM dbo.Users WHERE UserId = " + userId;

            int userIdFromDb = _dapper.LoadDataSingle<int>(userIdSql);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userIdFromDb)}
            });
        }
    }
}