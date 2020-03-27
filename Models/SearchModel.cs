using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FindMyKids.TeamService.Models
{
    public class SearchModel
    {
        [Required(AllowEmptyStrings = true)]
        public string UserName { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string FullName { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Email { get; set; }
        [Required(AllowEmptyStrings = true)]
        public string Type { get; set; }
        [Required]
        public int Page { get; set; }
    }
}