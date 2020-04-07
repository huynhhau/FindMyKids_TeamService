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
            var settings = new ConnectionSettings(new Uri(eLSOptions.Value.Uri))
                            .DefaultIndex(eLSOptions.Value.DefaultIndex)
                            .DefaultMappingFor<TeamService.Models.Children>(m => m
                                .IndexName("childrens")
                            );
            this.client = new ElasticClient(settings);
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
                    .Query(member.Email)
                    )
                )
            );

            if (searchResponse.Documents.Count > 0)
            {
                return searchResponse.Documents.FirstOrDefault();
            }

            return null;
        }

        public List<MemberInfo> Get(SearchModel search, int page, ref int total)
        {
            if (page != 0)
            {
                page = page - 1;
            }

            QueryContainer query = new MatchAllQuery();

            if (!string.IsNullOrEmpty(search.UserName))
            {
                query = query && new TermQuery
                {
                    Field = "userName",
                    Value = search.UserName
                };
            }

            if (!string.IsNullOrEmpty(search.FullName))
            {
                query = query && new TermQuery
                {
                    Field = "name",
                    Value = search.FullName
                };
            }

            //if (search.DateCreateFrom != null && search.DateCreateTo != null)
            //{
            //    query = query && new TermQuery
            //    {

            //        Field = "name",
            //        Value = search.FullName
            //    };


            //}

            //&& new TermQuery
            //{
            //    Field = "firstName",
            //    Value = "martijn"
            //}
            //&& new TermQuery
            //{
            //    Field = "firstName",
            //    Value = "martijn"
            //};

            //if (true)
            //{
            //    query = query && new TermQuery
            //    {
            //        Field = "firstName",
            //        Value = "martijn"
            //    };
            //}

            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                .From(0)
                .Size(PageInfo.PerPage)
                .Query(q => q.Match(m => m
                        .Field(f => f.UserName)
                        .Query(search.UserName))
                )

            );

            total = searchResponse.Documents.Count();

            return searchResponse.Documents.ToList().Skip(page * PageInfo.PerPage).Take(PageInfo.PerPage).ToList();
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

        public MemberInfo Get(string username)
        {
            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                                                                        .From(0)
                                                                        .Size(1)
                                                                        .Query(q => q
                                                                        .Match(m => m
                                                                                .Field(f => f.UserName)
                                                                                .Query(username)
                                                                                )
                                                                        )
                                                                    );

            if (searchResponse.Documents.Count > 0)
            {
                return searchResponse.Documents.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Lấy danh sách trẻ em theo mã cha/mẹ
        /// </summary>
        /// <param name="MemberId"></param>
        /// <returns></returns>
        public List<child> GetChildrens(Guid MemberId)
        {
            ISearchResponse<MemberInfo> searchResponse = this.client.Search<MemberInfo>(s => s
                                                            .From(0)
                                                            .Size(1)
                                                            .Query(q => q
                                                            .Match(m => m
                                                                    .Field(f => f.ID)
                                                                    .Query(MemberId.ToString())
                                                                    )
                                                            )
                                                        );


            if (searchResponse.Documents.Count > 0)
            {
                return searchResponse.Documents.FirstOrDefault().children;
            }

            return null;
        }

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

        public bool UpdateConnect(TeamService.Models.Children children, MemberInfo member)
        {
            if (member.children == null)
            {
                member.children = new List<child>();
            }

            IUpdateResponse<MemberInfo> update_child = client.Update<MemberInfo>(member.ID, u => u
                                    .Script(s => s
                                        .Source("ctx._source.children.add(params.child)")
                                        .Params(p => p
                                            .Add("child", new child
                                            {
                                                id = children.ID.ToString(),
                                                name = "",
                                                DateAdd = DateTime.Now,
                                                lat = children.lat,
                                                lon = children.lon
                                            })
                                        )
                                    )
                                    .Refresh(Refresh.True));

            IUpdateResponse<MemberInfo> update_PasswordConnect = client.Update<MemberInfo>(member.ID, u => u
                                    .Script(s => s
                                        .Source("ctx._source.passwordConnect = params.PasswordConnect")
                                        .Params(p => p
                                            .Add("PasswordConnect", "")
                                        )
                                    )
                                    .Refresh(Refresh.True));

            // Lưu thông tin children
            ISearchResponse<TeamService.Models.Children> searchResponse = this.client.Search<TeamService.Models.Children>(s => s
                .From(0)
                .Size(1)
                .Query(q => q
                .Match(m => m
                    .Field(f => f.ID)
                    .Query(children.ID.ToString())
                    )
                )
            );

            if (searchResponse.Documents.Count == 0)
            {
                children.DateAdd = DateTime.Now;
                IndexResponse indexResponse = this.client.IndexDocument(children);
                return update_child.IsValid && update_PasswordConnect.IsValid && indexResponse.IsValid;
            }

            return update_child.IsValid && update_PasswordConnect.IsValid;
        }

        public bool UpdatePasswordConnect(MemberInfo member)
        {
            IUpdateResponse<MemberInfo> updateRefreshToken = client.Update<MemberInfo>(member.ID, u => u
                                    .Script(s => s
                                        .Source("ctx._source.passwordConnect = params.PasswordConnect")
                                        .Params(p => p
                                            .Add("PasswordConnect", member.PasswordConnect)
                                        )
                                    )
                                    .Refresh(Refresh.True)
                                );

            //updateAccessToken.IsValid && 
            return updateRefreshToken.IsValid;
        }

        public bool UpdateState(string MemberId, string state)
        {
            IUpdateResponse<MemberInfo> updateRefreshToken = client.Update<MemberInfo>(MemberId, u => u
                        .Script(s => s
                            .Source("ctx._source.state = params.state")
                            .Params(p => p
                                .Add("state", state)
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