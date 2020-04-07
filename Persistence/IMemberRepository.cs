using FindMyKids.TeamService.Models;
using System;
using System.Collections.Generic;

namespace FindMyKids.FamilyService.Persistence
{
    public interface IMemberRepository
    {
        MemberInfo Get(Guid id);
        MemberInfo Get(string username);
        List<MemberInfo> Get(SearchModel searchModel, int page, ref int total);
        MemberInfo Get(AuthenticateModel auth);
        Member Add(Member member);
        bool Update(MemberInfo member);
        bool UpdatePasswordConnect(MemberInfo member);
        bool UpdateConnect(Children children, MemberInfo member);
        //bool UpdateLogout(MemberInfo member);
        Member Delete(Guid id);
        List<child> GetChildrens(Guid MemberId);
        bool UpdateState(string MemberId, string state);
        bool AddPlan(string id, plan obj);
    }
}