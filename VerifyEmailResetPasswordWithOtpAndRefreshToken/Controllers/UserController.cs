using Users.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using EmailService.Models;
using RefreshTokens;

namespace Users.Controllers
{
    [Route("Api/[Controller]")]
    [ApiController]

    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private readonly IConfiguration _config;
        private readonly IMailService mailService;
        public UserController(IMailService mailService, IConfiguration config, UserContext context)
        {
            _context = context;
            _config = config;
            this.mailService = mailService;
        }
        private User users = new User();
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDTO request)
        {
            if (_context.users.Any(u => u.Email == request.Email))
            {
                return BadRequest("Email already exists");
            }
            createPasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            users.Email = request.Email;
            users.Role = request.Role;
            users.PasswordHash = passwordHash;
            users.PasswordSalt = passwordSalt;
            users.VerificationToken = createVerificationToken();
            var subject = "Registration Successfull!";
            var body = $"You have been successfully registered as {users.Email}!<br>Your Verification Code is {users.VerificationToken}<br>kindly Verify your Email!";
            await mailService.sendEmailAsync(users, subject, body);
            _context.users.Add(users);
            await _context.SaveChangesAsync();

            return Ok("User Successfully Created!");
        }
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login(UserLoginDTO user)
        {
            // await _context.SaveChangesAsync();
            var findUser = await _context.users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if(findUser==null){
                    return BadRequest("User not found");
            }
            // foreach (var i in findUser)
            // {
                if (!verifyPassword(user.Password!, findUser.PasswordSalt!, findUser.PasswordHash!))
                {
                    return BadRequest("Your Password is wrong");
                }
                if (findUser.VerifiedAt == null)
                {
                    return BadRequest("Kindly verify your Email!");
                }
                Console.WriteLine("############" + findUser.VerificationToken);
                var token = CreateToken(findUser);
                var refreshtoken = GenerateRefreshToken(findUser);
                setRefreshtoken(findUser, refreshtoken);
                findUser.RefreshToken = refreshtoken.refreshtoken;
                findUser.tokenCreated = refreshtoken.Created;
                findUser.tokenExpires = refreshtoken.Expired;
                await _context.SaveChangesAsync();
                // updateUsers(i);
                return Ok("You have been successfully Logged in.\nYour Token is : " + token);
            // }


            return BadRequest("Account doesnt exist");
        }
          private RefreshToken GenerateRefreshToken(User users)
        {
            var refreshToken = new RefreshToken
            {
                refreshtoken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expired = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };
            return refreshToken;
        }
         [NonAction]
        public void updateUsers(User user)
        {
            // var users = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            // _context.Update(user);

            _context.Entry(user).State = EntityState.Modified;

            _context.SaveChanges();
        }

        private void setRefreshtoken(User user, RefreshToken newrefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newrefreshToken.Expired

            };
            Response.Cookies.Append("RefreshToken", newrefreshToken.refreshtoken, cookieOptions);
        }

        [HttpPost("Refresh-Token")]
        public async Task<ActionResult<string>> refreshToken(){
            var refreshToken=Request.Cookies["refreshToken"];
            Console.WriteLine("************"+refreshToken);
            // if(refreshToken==null){
            //     return Unauthorized("Refresh Token expired");
            // }
            var user=await _context.users.FirstOrDefaultAsync(u=> u.RefreshToken==refreshToken);
            Console.WriteLine("***********"+user.RefreshToken);
            if(user==null){
                return Unauthorized("Invalid Refresh token");
            }
         
            if(user.tokenExpires<DateTime.Now){
                return Unauthorized("RefreshToken expired");
            }
            string token=CreateToken(user);
            var newrefreshToken=GenerateRefreshToken(users);
            user.RefreshToken=newrefreshToken.refreshtoken;
            user.tokenCreated=newrefreshToken.Created;
            user.tokenExpires=newrefreshToken.Expired;
            setRefreshtoken(users,newrefreshToken);
            updateUsers(user);
            return Ok(token);

        }


        [HttpPost("verify")]
        public async Task<ActionResult> Verify(string token)
        {
            var findUser = await _context.users.FirstOrDefaultAsync(u => u.VerificationToken == token);
            if (findUser == null)
            {
                return BadRequest("Invalid Token");
            }
            findUser.VerifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok("User verified!");
        }
        [HttpPost("Forgot-Password")]
        public async Task<IActionResult> forgotPassword(string email)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User not registered!");
            }
            user.ResetPasswordToken = createVerificationToken();
            user.ResetTokenExpiresAt = DateTime.Now.AddDays(1);
            var subject = "verification Token";
            var body = $"You can now reset your password!<br>Your OTP to reset your password is {user.ResetPasswordToken}<br>You can Reset your password!<br>Your OTP will expire in 1 day!";
            await mailService.sendEmailAsync(user, subject, body);
            await _context.SaveChangesAsync();
            return Ok("You can reset your password now");
        }
        [HttpPost("Reset Password")]
        public async Task<IActionResult> resetPassword(ResetPassword resetRequest)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.ResetPasswordToken == resetRequest.resettoken);
            if (user == null || user.ResetTokenExpiresAt < DateTime.Now)
            {
                return BadRequest("Invalid Token");
            }
            createPasswordHash(resetRequest.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetPasswordToken = null;
            user.ResetTokenExpiresAt = null;
            await _context.SaveChangesAsync();
            return Ok("Your password has been reset");
        }

        private void createPasswordHash(string password, out byte[] passwordhash, out byte[] passwordsalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordsalt = hmac.Key;
                passwordhash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private string createVerificationToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(4));
        }
        private bool verifyPassword(string password, byte[] passwordsalt, byte[] passwordhash)
        {
            using (var hmac = new HMACSHA512(passwordsalt))
            {
                var computedhash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                return computedhash.SequenceEqual(passwordhash);
            }
        }
        private string CreateToken(User users)
        {
            List<Claim> claims = new List<Claim>(){
                new Claim(ClaimTypes.Email,users.Email),
                new Claim(ClaimTypes.Role,users.Role)
            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("Appsettings:key").Value));
            Console.WriteLine(key);
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            Console.WriteLine(cred);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddSeconds(0.5),
                signingCredentials: cred);
            // Console.WriteLine(token);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            // Console.WriteLine(jwt);
            return jwt;
        }
    }
}