using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Models.ViewModels;
using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public interface IHistologyService
    {
        HistologyEditViewModel FillEditViewModel(string NHSnumber);
        List<HistologyLookup> GetLookups(string lookupType);
        int SaveHeader(HistologyEditViewModel model);
        HistologyFocusViewModel FillEditFocusViewModel(string NHSnumber, int headerId, int primary, int focusId);
        int SaveFocus(HistologyFocusViewModel model);
    }
}
