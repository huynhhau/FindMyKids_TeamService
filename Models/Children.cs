using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FindMyKids.TeamService.Models
{
    public class Children
    {
        [Required(AllowEmptyStrings = true)]
        public Guid? ID { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string AccessToken { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string RefreshToken { get; set; }
        [Required]
        public string ParentID { get; set; }
        [Required]
        public string PasswordConnect { get; set; }
        public DateTime? DateAdd { get; set; }
        public double? lat { get; set; }
        public double? lon { get; set; }
    }
}
