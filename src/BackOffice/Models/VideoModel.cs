using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BackOffice.Models
{
    public class VideoModel
    {
        public VideoModel()
        {
            Id = Guid.NewGuid();
        }

        [HiddenInput]
        public Guid Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Name : ")]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description : ")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Choose a file : ")]
        public HttpPostedFileBase File { get; set; }
    }
}
