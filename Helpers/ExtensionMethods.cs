using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindMyKids.TeamService.Models
{
    public static class ExtensionMethods
    {
        public static IEnumerable<MemberInfo> WithoutPasswords(this IEnumerable<MemberInfo> users)
        {
            return users.Select(x => x.WithoutPassword());
        }

        public static MemberInfo WithoutPassword(this MemberInfo user)
        {
            user.PassWord = string.Empty;
            return user;
        }
    }
}
