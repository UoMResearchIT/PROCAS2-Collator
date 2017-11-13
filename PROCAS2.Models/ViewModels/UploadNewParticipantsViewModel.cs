using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PROCAS2.Models.ViewModels
{
    public class UploadNewParticipantsViewModel
    {
        [Display(Name ="Please enter a file name: ")]
        [Required(ErrorMessage = "Please enter a file name")]
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
