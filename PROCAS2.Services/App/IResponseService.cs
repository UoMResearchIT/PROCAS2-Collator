using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PROCAS2.Data.Entities;

namespace PROCAS2.Services.App
{
    public interface IResponseService
    {
        bool CreateQuestionnaireHeader(string patientId, string dateStarted, string dateFinished, out QuestionnaireResponse response);
        bool CreateResponseItem(QuestionnaireResponse response, string questionCode, string answerText);
        bool CreateFamilyHistoryItem(QuestionnaireResponse response, FamilyHistoryItem familyHistoryItem);
    }
}
