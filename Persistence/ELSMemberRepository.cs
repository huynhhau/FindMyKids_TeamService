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