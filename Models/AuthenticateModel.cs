using System.ComponentModel.DataAnnotations;

namespace FindMyKids.TeamService.Models
{
    public class AuthenticateModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string PassWord { get; set; }
    }
}
