using FindMyKids.FamilyService.Persistence;
using FindMyKids.TeamService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;

namespace FindMyKids.TeamService
{
    [Route("[controller]")]
    public class TeamsController : Controller
    {
        IMemberRepository repository;
        string secret = string.Empty;
        string reCaptcha = string.Empty;

        public TeamsController(IMemberRepository repo, IOptions<AppSettings> appOptions)
        {
            repository = repo;
            secret = appOptions.Value.Secret;
            reCaptcha = appOptions.Value.ReCaptcha;
        }

        [HttpGet("/[controller]/password")]
        public virtual IActionResult GetPassWordConnect()
        {
            Guid id = new Guid("4d394c1e-dcbe-4a7f-a25b-752eabbe99b3");
            MemberInfo memberInfo = repository.Get(id);

            if (string.IsNullOrEmpty(memberInfo.PasswordConnect))
            {
                var key = KeyGeneration.GenerateRandomKey(10);
                var base32String = Base32Encoding.ToString(key);
                memberInfo.PasswordConnect = base32String.Substring(0, 6);
                if (repository.UpdatePasswordConnect(memberInfo))
                {
                    return this.Ok(base32String.Substring(0, 6));
                }
                else
                {
                    return this.NotFound();
                }
            }

            return this.Ok(memberInfo.PasswordConnect);
        }

        [HttpPost("/[controller]/connect")]
        public virtual IActionResult Connect([FromBody]Children children)
        {
            if (string.IsNullOrEmpty(children.ID.ToString()))
            {
                children.ID = Guid.NewGuid();

                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, children.ID.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(5),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                children.AccessToken = tokenHandler.WriteToken(token);

                children.RefreshToken = Guid.NewGuid().ToString();
            }

            MemberInfo memberInfo = repository.Get(children.ParentID);

            if (memberInfo != null)
            {
                if (memberInfo.PasswordConnect == children.PasswordConnect)
                {
                    // Kiểm tra xem con có trong danh sách
                    if (memberInfo.children != null &&
                        memberInfo.children.Select(chld => chld.id == children.ID.ToString()).Count() > 0)
                    {
                        return this.NotFound();
                    }

                    if (repository.UpdateConnect(children, memberInfo))
                    {
                        return this.Ok(children);
                    }
                }
                else
                {
                    return this.NotFound();
                }
            }

            return this.NotFound();
        }

        //[HttpGet]
        //      public virtual IActionResult GetAllTeams()
        //{
        //	return this.Ok(repository.List());
        //}

        //[HttpGet("{id}")]
        //      public IActionResult GetTeam(Guid id)
        //{
        //	Team team = repository.Get(id);		

        //	if (team != null) // I HATE NULLS, MUST FIXERATE THIS.			  
        //	{				
        //		return this.Ok(team);
        //	} else {
        //		return this.NotFound();
        //	}			
        //}		

        //[HttpPost]
        //public virtual IActionResult CreateTeam([FromBody]Team newTeam) 
        //{
        //	repository.Add(newTeam);			

        //	//TODO: add test that asserts result is a 201 pointing to URL of the created team.
        //	//TODO: teams need IDs
        //	//TODO: return created at route to point to team details			
        //	return this.Created($"/teams/{newTeam.ID}", newTeam);
        //}

        //[HttpPut("{id}")]
        //public virtual IActionResult UpdateTeam([FromBody]Team team, Guid id) 
        //{
        //	team.ID = id;

        //	if(repository.Update(team) == null) {
        //		return this.NotFound();
        //	} else {
        //		return this.Ok(team);
        //	}
        //}

        //[HttpDelete("{id}")]
        //      public virtual IActionResult DeleteTeam(Guid id)
        //{
        //	Team team = repository.Delete(id);

        //	if (team == null) {
        //		return this.NotFound();
        //	} else {				
        //		return this.Ok(team.ID);
        //	}
        //}
    }
}
