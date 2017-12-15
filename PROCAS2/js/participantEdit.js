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

});


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