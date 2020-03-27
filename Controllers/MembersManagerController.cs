using FindMyKids.FamilyService.Persistence;
using FindMyKids.TeamService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace FindMyKids.TeamService.Controllers
{
	[Route("[controller]")]
    public class MembersManagerController: Controller
    {
        IMemberRepository repository;

        public MembersManagerController(IMemberRepository repo, IOptions<AppSettings> appOptions)
        {
            repository = repo;
        }

		[AllowAnonymous]
		[HttpGet]
		[EnableCors("_myAllowSpecificOrigins")]
		[Route("/[controller]/manager/members")]
		public virtual IActionResult getList()
		{
			return this.Ok(repository.Get(new SearchModel()));
		}
	}
}
