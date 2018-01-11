using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QY.Admin.Logic.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        [Display(Name ="中文名")]
        public string ChineseName { get; set; }

        [Required]
        [StringLength(maximumLength: 100)]
        [Display(Name = "英文名（拼音全称）")]
        public string EnglishName { get; set; }

        [Display(Name ="邮箱")]
        public string EmailAddress { get; set; }

        public bool? IsManager { get; set; }

        [Display(Name ="部门")]
        public string department { get; set; }

        public bool? IsExcluded { get; set; }

        public DateTime? CreatedTime { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public string UpdatedBy { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
