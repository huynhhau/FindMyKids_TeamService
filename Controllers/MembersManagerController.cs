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
		[HttpPost]
		[EnableCors("_myAllowSpecificOrigins")]
		[Route("/[controller]/manager/members")]
		public virtual IActionResult getList([FromBody]SearchModel searchModel, [FromQuery]int page)
		{
			int total = 0;
			List<MemberInfo> memberInfos = repository.Get(searchModel, page, ref total);
			return this.Ok(new
			{
				total = total,
				memberInfos = memberInfos
			}) ;
		}

		[AllowAnonymous]
		[HttpPost]
		[EnableCors("_myAllowSpecificOrigins")]
		[Route("/[controller]/manager/members/updatestate/{memberId}/{state}")]
		public bool updateState(string memberId, string state)
		{
			return repository.UpdateState(memberId, state);
		}
	}
}
