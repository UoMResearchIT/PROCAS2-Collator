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
    public class UploadAskRiskViewModel
    {
        [Display(Name = "UPLOAD_ASK_RISK_FILE_NAME", ResourceType = typeof(UploadResources))]
        [Required(ErrorMessageResourceName = "UPLOAD_ASK_RISK_FILE_NAME", ErrorMessageResourceType = typeof(UploadResources))]
        public HttpPostedFileBase UploadedFile { get; set; }
    }
}
