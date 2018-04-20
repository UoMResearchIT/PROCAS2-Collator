$(document).ready(function () {

    $(".datepicker").datepicker({
        format: "dd/mm/yyyy"
    });


    // When saving the participant record prompt for a reason for the change. This reason will get
    // added to the audit trail information.
    $("#btnSave").on("click", function () {

        var self = this;

        $.confirm({
            title: 'Reason for Change',
            content: '' +
            '<form action="" class="formName">' +
            '<div class="form-group">' +
            '<label>Please enter a reason for making this change.</label>' +
            '<input type="text" placeholder="Your reason" class="name form-control" required />' +
            '</div>' +
            '</form>',
            buttons: {
                formSubmit: {
                    text: 'Submit',
                    btnClass: 'btn-blue',
                    action: function () {
                        var name = this.$content.find('.name').val();
                        if (!name) {
                            $.alert('Please provide a reason');
                            return false;
                        }
                        
                        $("#Reason").val(name);
                        $("#frmEdit").submit();
                    }
                },
                cancel: function () {
                    //close
                },
            },
            onContentReady: function () {
                // bind to events
                var jc = this;
                this.$content.find('form').on('submit', function (e) {
                    // if the user submits the form by pressing enter in the field.
                    e.preventDefault();
                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                });
            }
        });
    });


    // Delete will remove all the information entirely, so make sure they mean it!
    $("#btnDelete").on("click", function () {
        $.confirm({
            title: 'Delete participant',
            content: "This will remove all the participant's data from BC-PREDICT. Are you sure you want to do that? This action cannot be undone.",
            buttons: {
                confirm: function () {
                    $("#frmDelete").submit();
                },
                cancel: function () {
                    
                }                
            }
            
        });
    });

    // If the Chemo Agreed flag changes then toggle the drop-down box
    $("#ChemoAgreedInClinic").on("change", function () {
        setChemoBox();
    });

    // If the Initial screening outcome flag changes then toggle the drop-down box
    $("#InitialScreeningOutcomeId").on("change", function () {
        setScreeningOutcome();
    });

    // If the Risk Consultation Completed flag changes then toggle the drop-down box
    $("#RiskConsultationCompleted").on("change", function () {
        setRiskConsultation();
    });


    setChemoBox();
    setScreeningOutcome();
    setRiskConsultation();

});

// Check the Chemo Agreed In Clinic flag and enable or disable the Chemo details dropdown appropriately
function setChemoBox()
{
    var checked = $("#ChemoAgreedInClinic").prop("checked");
    if (checked === true){
        $("#ChemoPreventionDetailsId").prop("disabled", false);
    }
    else {
        $("#ChemoPreventionDetailsId").prop("disabled", true);
        $("#ChemoPreventionDetailsId").val(null);
    }
}

// Check the Risk Consultation Completed flag and enable or disable the risk consultation dropdown appropriately
function setRiskConsultation() {
    var checked = $("#RiskConsultationCompleted").prop("checked");
    if (checked === true) {
        $("#RiskConsultationTypeId").prop("disabled", false);
        $("#RiskConsultationLetterSent").prop("disabled", false);
    }
    else {
        $("#RiskConsultationTypeId").prop("disabled", true);
        $("#RiskConsultationTypeId").val(null);
        $("#RiskConsultationLetterSent").prop("disabled", true);
        $("#RiskConsultationLetterSent").prop("checked", false);
    }
}

// Check the initial screening outcome box and enable or disable the appropriate final screening outcome boxes
function setScreeningOutcome()
{
    var initial = $("#InitialScreeningOutcomeId > option:selected").text();

    if (initial.substr(0, 1) == "1") {
        $("#FinalTechnicalOutcomeId").prop("disabled", false);
        $("#FinalAssessmentOutcomeId").prop("disabled", true);
        $("#FinalAssessmentOutcomeId").val(null);
    }
    else if (initial.substr(0, 1) == "2") {
        $("#FinalTechnicalOutcomeId").prop("disabled", true);
        $("#FinalTechnicalOutcomeId").val(null);
        $("#FinalAssessmentOutcomeId").prop("disabled", false);
    }
    else {
        $("#FinalTechnicalOutcomeId").prop("disabled", true);
        $("#FinalTechnicalOutcomeId").val(null);
        $("#FinalAssessmentOutcomeId").prop("disabled", true);
        $("#FinalAssessmentOutcomeId").val(null);
    }

}

// Handle the returned delete message (true or false)
function onDeleteSuccess(result) {

    if (result.deleted === true) {
        // Redirect to list
        window.location.href = "/Participant/Index";
    }
    else {
        $.alert({
            title: 'Not Deleted',
            content: 'There was a problem and the participant has not been deleted.',
        });
    }

}