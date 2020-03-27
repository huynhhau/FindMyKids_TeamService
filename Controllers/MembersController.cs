using FindMyKids.FamilyService.Persistence;
using FindMyKids.TeamService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FindMyKids.TeamService
{
	[Route("[controller]")]
	public class MembersController : Controller
	{
		IMemberRepository repository;
		string secret = string.Empty;
		string reCaptcha = string.Empty;

		public MembersController(IMemberRepository repo, IOptions<AppSettings> appOptions) 
		{
			repository = repo;
			secret = appOptions.Value.Secret;
			reCaptcha = appOptions.Value.ReCaptcha;
		}

		//public MembersController(IMemberRepository repo, AppSettings appOptions)
		//{
		//	repository = repo;
		//	secret = appOptions.Secret;
		//	reCaptcha = appOptions.ReCaptcha;
		//}

		[AllowAnonymous]
		[HttpPost]
		[EnableCors("_myAllowSpecificOrigins")]
		[Route("/[controller]/login")]
		public virtual IActionResult Login([FromBody]AuthenticateModel auth)
		{
			//if (Request.Headers.ContainsKey("recaptchaToken"))
			//{
			//	string EncodeResponse = Request.Headers["recaptchaToken"];
			//	if (EncodeResponse == null)
			//	{
			//		return this.NotFound();
			//	}

			//	if (!Recaptcha.Validate(EncodeResponse, reCaptcha))
			//	{
			//		return this.NotFound();
			//	}
			//}
			//else
			//{
			//	return this.NotFound();
			//}

			MemberInfo member = repository.Get(auth);
			if (member != null && BCrypt.Net.BCrypt.Verify(auth.PassWord, member.PassWord))
			{
				// authentication successful so generate jwt token
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(secret);
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new Claim[]
					{
						new Claim(ClaimTypes.Name, member.ID.ToString())
					}),
					Expires = DateTime.UtcNow.AddDays(5),
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
				};
				var token = tokenHandler.CreateToken(tokenDescriptor);

				string accessToken = tokenHandler.WriteToken(token);

				if (string.IsNullOrEmpty(member.RefreshToken))
				{
					member.RefreshToken = Guid.NewGuid().ToString();
				}

				if (repository.Update(member))
				{
					return this.Ok(new
					{
						authenticated = true,
						Id = member.ID,
						Name = member.Name,
						AccessToken = accessToken,
						RefreshToken = member.RefreshToken
					});
				}
			}

			return this.Ok(new
			{
				authenticated = false
			});
		}

		[AllowAnonymous]
		[HttpPost]
		[EnableCors("_myAllowSpecificOrigins")]
		[Route("/[controller]/{idMember}/renew-token/{rfToken}")]
		public virtual IActionResult renewToken(Guid idMember, string rfToken)
		{
			MemberInfo member = repository.Get(idMember);
			if (member != null && member.RefreshToken == rfToken && !string.IsNullOrEmpty(member.RefreshToken))
			{
				// authentication successful so generate jwt token
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(secret);
				var tokenDescriptor = new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(new Claim[]
					{
						new Claim(ClaimTypes.Name, member.ID.ToString())
					}),
					Expires = DateTime.UtcNow.AddSeconds(5),
					SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
				};
				var token = tokenHandler.CreateToken(tokenDescriptor);

				string accessToken = tokenHandler.WriteToken(token);

				if (repository.Update(member))
				{
					return this.Ok(new
					{
						authenticated = true,
						AccessToken = accessToken
					});
				}
			}

			return this.Ok(new
			{
				authenticated = false
			});
		}

		//[HttpPost]
		//[EnableCors("_myAllowSpecificOrigins")]
		//[Route("/[controller]/logout/{memberId}")]
		//public virtual IActionResult Logout(Guid memberId)
		//{
		//	MemberInfo member = repository.Get(memberId);
		//	if (member != null)
		//	{
		//		if (repository.UpdateLogout(member))
		//		{
		//			return this.Ok(new
		//			{
		//				logout = true
		//			});
		//		}
		//	}

		//	return this.Ok(new
		//	{
		//		logout = false
		//	});
		//}

		[Authorize]
		[HttpPost]
		[EnableCors("_myAllowSpecificOrigins")]
		public virtual IActionResult CreateMember([FromBody]Member newMember)
		{
			if (Request.Headers.ContainsKey("recaptchaToken"))
			{
				string EncodeResponse = Request.Headers["recaptchaToken"];
				if (EncodeResponse == null)
				{
					return this.NotFound();
				}

				if (!Recaptcha.Validate(EncodeResponse, reCaptcha))
				{
					return this.NotFound();
				}
			}
			else
			{
				return this.NotFound();
			}

			newMember.PassWord = BCrypt.Net.BCrypt.HashPassword(newMember.PassWord);
			if (repository.Add(newMember) != null)
			{
				return this.Created($"[controller]", newMember);
			}

			return this.NotFound();
		}

		//[HttpGet]
		//public virtual IActionResult GetMembers(Guid teamID) 
		//{
		//	//Team team = repository.Get(teamID);

		//	//if(team == null) {
		//	//	return this.NotFound();
		//	//} else {
		//	//	return this.Ok(team.Members);
		//	//}
		//	return this.NotFound();
		//}


		//[HttpGet]
		//[Route("/teams/{teamId}/[controller]/{memberId}")]		
		//public virtual IActionResult GetMember(Guid teamID, Guid memberId) 
		//{
		//	//Team team = repository.Get(teamID);

		//	//if(team == null) {
		//	//	return this.NotFound();
		//	//} else {
		//	//	var q = team.Members.Where(m => m.ID == memberId);

		//	//	if(q.Count() < 1) {
		//	//		return this.NotFound();
		//	//	} else {
		//	//		return this.Ok(q.First());
		//	//	}				
		//	//}			

		//	return this.NotFound();
		//}

		//[HttpPut]
		//[Route("/teams/{teamId}/[controller]/{memberId}")]		
		//public virtual IActionResult UpdateMember([FromBody]Member updatedMember, Guid teamID, Guid memberId) 
		//{
		//	//Team team = repository.Get(teamID);

		//	//if(team == null) {
		//	//	return this.NotFound();
		//	//} else {
		//	//	var q = team.Members.Where(m => m.ID == memberId);

		//	//	if(q.Count() < 1) {
		//	//		return this.NotFound();
		//	//	} else {
		//	//		team.Members.Remove(q.First());
		//	//		team.Members.Add(updatedMember);
		//	//		return this.Ok();
		//	//	}
		//	//}
		//	return this.Ok();
		//}

		//[HttpGet]
		//[Route("/members/{memberId}/team")]
		//public IActionResult GetTeamForMember(Guid memberId)
		//{
		//	//var teamId = GetTeamIdForMember(memberId);
		//	//if (teamId != Guid.Empty) {
		//	//	return this.Ok(new {
		//	//		TeamID = teamId
		//	//	});
		//	//} else {
		//	//	return this.NotFound();
		//	//}

		//	return this.NotFound();
		//}

		//private Guid GetTeamIdForMember(Guid memberId)
		//{
		//	//foreach (var team in repository.List()) {
		//	//	var member = team.Members.FirstOrDefault( m => m.ID == memberId);
		//	//	if (member != null) {
		//	//		return team.ID;
		//	//	}
		//	//}
		//	return Guid.Empty;
		//}    
	}
}