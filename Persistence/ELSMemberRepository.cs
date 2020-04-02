using Elasticsearch.Net;
using FindMyKids.FamilyService.Models;
using FindMyKids.TeamService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FindMyKids.FamilyService.Persistence
{
    public class ELSMemberRepository : IMemberRepository
    {
        private readonly ILogger logger;
        private ElasticClient client = null;

        public ELSMemberRepository(ILogger<ILogger> logger, IOptions<ELSOptions> eLSOptions)
        {
            this.logger = logger;
            this.client = new ElasticClient(new ConnectionSettings(new Uri(eLSOptions.Value.Uri))
                                          .DefaultIndex(eLSOptions.Value.DefaultIndex));
        }

        public Member Add(Member member)
        {
            ISearchResponse<Member> searchResponse = this.client.Search<Member>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                .Match(m => m
                    .Field(f => f.UserName)
                    .Query(member.UserName)
                    )
                )
            );

            if (searchResponse.Documents.Count == 0)
            {
                member.ID = Guid.NewGuid();
                this.client.IndexDocument(member);
                return member;
            }

            return null;
        }

        public bool AddPlan(string id, plan obj)
        {
            IUpdateResponse<Member> update_plan = client.Update<Member>(id, u => u
                                  .Script(s => s
                                      .Source("ctx._source.plans.add(params.plan)")
                                      .Params(p => p
                                          .Add("plan", new plan
                                          {
                                              id = obj.id,
                                              name = obj.name,
                                              status = obj.status,
                                              description = obj.description,
                                              create_time = obj.create_time,
                                              price = obj.price,
                                              links = obj.links
                                          })
                                      )
                                  )
                                  .Refresh(Refresh.True));
            return update_plan.IsValid;
        }

        public Member Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public MemberInfo Get(AuthenticateModel member)
        {
            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                .Match(m => m
                    .Field(f => f.UserName)
                    .Query(member.UserName)
                    )
                )
            );

            if (searchResponse.Documents.Count > 0)
            {
                return searchResponse.Documents.FirstOrDefault();
            }

            return null;
        }

        public List<MemberInfo> Get(SearchModel search)
        {
            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                .From(search.Page * PageInfo.PerPage)
                .Size(PageInfo.PerPage)
                .MatchAll()
            );

            return searchResponse.Documents.ToList();
        }

        public MemberInfo Get(Guid id)
        {
            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                .Match(m => m
                        .Field(f => f.ID)
                        .Query(id.ToString())
                        )
                )
            );

            if (searchResponse.Documents.Count > 0)
            {
                return searchResponse.Documents.FirstOrDefault();
            }

            return null;
        }

        public plan getPlan(string idPlan)
        {
            throw new NotImplementedException();
        }

        //public plan getPlan(string idPlan)
        //{

        //}

        public bool Update(MemberInfo member)
        {
            //IUpdateResponse<MemberInfo> updateAccessToken = client.Update<MemberInfo>(member.ID, u => u
            //                                    .Script(s => s
            //                                        .Source("ctx._source.AccessToken = params.AccessToken")
            //                                        .Params(p => p
            //                                            .Add("AccessToken", member.AccessToken)
            //                                        )
            //                                    )
            //                                    .Refresh(Refresh.True)
            //                                );

            IUpdateResponse<MemberInfo> updateRefreshToken = client.Update<MemberInfo>(member.ID, u => u
                                                .Script(s => s
                                                    .Source("ctx._source.refreshToken = params.RefreshToken")
                                                    .Params(p => p
                                                        .Add("RefreshToken", member.RefreshToken)
                                                    )
                                                )
                                                .Refresh(Refresh.True)
                                            );

            //updateAccessToken.IsValid && 
            return updateRefreshToken.IsValid;
        }

        //public bool UpdateLogout(MemberInfo member)
        //{
        //    IUpdateResponse<MemberInfo> updateAccessToken = client.Update<MemberInfo>(member.ID, u => u
        //                                        .Script(s => s
        //                                            .Source("ctx._source.AccessToken = params.AccessToken")
        //                                            .Params(p => p
        //                                                .Add("AccessToken", "")
        //                                            )
        //                                        )
        //                                        .Refresh(Refresh.True)
        //                                    );

        //    IUpdateResponse<MemberInfo> updateRefreshToken = client.Update<MemberInfo>(member.ID, u => u
        //                                        .Script(s => s
        //                                            .Source("ctx._source.RefreshToken = params.RefreshToken")
        //                                            .Params(p => p
        //                                                .Add("RefreshToken", "")
        //                                            )
        //                                        )
        //                                        .Refresh(Refresh.True)
        //                                    );

        //    return updateAccessToken.IsValid && updateRefreshToken.IsValid;
        //}
    }
}