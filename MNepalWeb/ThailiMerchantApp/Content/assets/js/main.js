var ThailiJS = {
    init: function() {
        ThailiJS.thailiSlider(); /** Implemente matchHeight **/
        ThailiJS.setEqualHeight(); /** Call Match height js **/
        ThailiJS.formValidation(); /** Basic form validation **/
        ThailiJS.selectJs(); /** Implement bootstrap select **/
        ThailiJS.inputDatepicker(); /** Initialize datepicker **/
        ThailiJS.inputDateRange(); /** Input date range **/
        ThailiJS.uploadFunction(); /** Custom file upload function **/
        ThailiJS.showPasswordFunction(); /** View pasword text **/
    },

    /** Implemente matchHeight **/
    thailiSlider: function() {
        var slider = $('.thaili-slider');

        if (slider.length > 0) {
            slider.bxSlider({
                controls: false,
                auto: true,
                pause: 6000
            });
        }
    },

    setEqualHeight: function() {
        var container = $('.match-height');

        if (container.length > 0) {
            setTimeout(function() {
                container.matchHeight({
                    byRow: true,
                });
            }, 1000);
        }
    },

    formValidation: function() {
        var form = $('.common-form');

        if (form.length > 0) {
            // console.log('test form');
            form.validate();
        }
    },

    selectJs: function() {
        var select = $('.selectpicker');

        if (select.length > 0) {
            select.selectpicker({
                iconBase: 'thaili-fonts'
            });
        }
    },

    inputDatepicker: function() {
        var input = $('.datepicker-input');

        if(input.length > 0) {
            input.datepicker({
                format: 'dd-mm-yyyy',
                endDate: '0d',
                autoclose: true
            });

            input.next('.input-group-append').on('click', function(evt) {
                input.trigger('focus');
            });
        }
    },

    inputDateRange: function() {
        var dateRange = $('.daterange-wrapper');

        if (dateRange.length > 0) {
            dateRange.find('.input-daterange').each(function() {
                $(this).datepicker('clearDates');
            });
        }
    },

    uploadFunction: function() {
        var uploadContainer = $('.upload-input-wrapper');

        if (uploadContainer.length > 0) {

            uploadContainer.each(function(index, element) {
                var current = $(element);
                var fileInput = current.find('input[type=file]');
                var uploadLabel = current.find('label.upload-label');

                fileInput.on('change', function() {
                    var value = fileInput.val();

                    if (value != '') {
                        uploadLabel.text(value);
                    } else {
                        uploadLabel.text(uploadLabel.data('text'));
                    }
                });
            });
        }
    },

    showPasswordFunction: function() {
        var showPassword = $('.show-password');

        if (showPassword.length > 0) {

            showPassword.on('mousedown', function() {
                var current = $(this);
                var passwordInput = current.parents('.form-group').find('input[type=password]');
                passwordInput.attr('type', 'text');
            });

            showPassword.on('mouseup', function() {
                var current = $(this);
                var passwordInput = current.parents('.form-group').find('input[type=text]');
                passwordInput.attr('type', 'password');
            });

            showPassword.on('mouseleave', function() {
                var current = $(this);
                var passwordInput = current.parents('.form-group').find('input[type=text]');
                passwordInput.attr('type', 'password');
            });
        }
    }
}

document.addEventListener("DOMContentLoaded", function(event) {
    myInitCode();
});

function myInitCode() {
    ThailiJS.init();
}
