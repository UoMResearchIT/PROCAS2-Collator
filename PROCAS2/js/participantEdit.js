$(document).ready(function () {

    $(".datepicker").datepicker({
        format: "dd/mm/yyyy"
    });

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

});
