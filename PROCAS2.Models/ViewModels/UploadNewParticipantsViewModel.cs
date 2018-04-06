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
    public class UploadNewParticipantsViewModel
    {
        
        [Display(Name = "UPLOAD_NEW_FILE_NAME", ResourceType = typeof(UploadResources))]
        [Required(ErrorMessageResourceName = "UPLOAD_NEW_FILE_NAME", ErrorMessageResourceType = typeof(UploadResources))]
        public HttpPostedFileBase UploadedFile { get; set; }

        [Display(Name = "UPLOAD_NEW_REGENERATE", ResourceType = typeof(UploadResources))]
        public bool Regenerate { get; set; }
    }
}
