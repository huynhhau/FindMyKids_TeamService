using FindMyKids.TeamService.Models;
using System;
using System.Collections.Generic;

namespace FindMyKids.FamilyService.Persistence
{
    public interface IMemberRepository
    {
        MemberInfo Get(Guid id);
        List<MemberInfo> Get(SearchModel searchModel);
        MemberInfo Get(AuthenticateModel auth);
        Member Add(Member member);
        bool Update(MemberInfo member);
        //bool UpdateLogout(MemberInfo member);
        Member Delete(Guid id);
    }
}