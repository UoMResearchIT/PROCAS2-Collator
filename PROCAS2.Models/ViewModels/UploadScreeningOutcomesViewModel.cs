using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.ComponentModel.DataAnnotations;

using PROCAS2.Resources;

namespace PROCAS2.Models.ViewModels
{
    public class UploadScreeningOutcomesViewModel
    {
        [Display(Name = "UPLOAD_OUTCOMES_FILE_NAME", ResourceType = typeof(UploadResources))]
        [Required(ErrorMessageResourceName = "UPLOAD_OUTCOMES_FILE_NAME", ErrorMessageResourceType = typeof(UploadResources))]
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
