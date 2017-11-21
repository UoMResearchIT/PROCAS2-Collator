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
    public class UploadUpdateParticipantsViewModel
    {
        [Display(Name = "VM_UPLOAD_UPDATE_FILE_NAME", ResourceType = typeof(PROCASRes))]
        [Required(ErrorMessageResourceName = "VM_UPLOAD_UPDATE_FILE_NAME", ErrorMessageResourceType = typeof(PROCASRes))]
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
