
jQuery(document).ready(function ($) {
    //$('#BSCitizenshipIssueDate, #BSLicenseIssueDate, #BSLicenseExpireDate').hide();

    $('#BSCitizenshipIssueDate, #BSLicenseIssueDate,#BSDateOfBirth').nepaliDatePicker({
        npdMonth: true,
        npdYear: true,
        npdYearCount: 100, // Options | Number of years to show
        ndpEnglishInput: 'DOB1',
        disableDaysAfter: '1',
        
        onChange: function () {
            if ($('#BSCitizenshipIssueDate').val() != '') {
                $('#CitizenshipIssueDate').val(GetShowFormattedDate(BS2AD($('#BSCitizenshipIssueDate').val())));

            }
            debugger;
            if ($('#BSLicenseIssueDate').val() != '') {
                $('#LicenseIssueDate').val(GetShowFormattedDate(BS2AD($('#BSLicenseIssueDate').val())));
            } 
            if ($('#BSDateOfBirth').val() != '') {
                $('#DOB').val(GetShowFormattedDate(BS2AD($('#BSDateOfBirth').val())));
            } 

        }
    });
    $('#BSLicenseExpireDate').nepaliDatePicker({
        npdMonth: true,
        npdYear: true,
        npdYearCount: 100, // Options | Number of years to show
        ndpEnglishInput: 'DOB1',
        disableDaysBefore: '1',
        
        onChange: function () {
         
            if ($('#BSLicenseExpireDate').val() != '') {
                $('#LicenseExpireDate').val(GetShowFormattedDate(BS2AD($('#BSLicenseExpireDate').val())));

            }
           

        }
    });
    $('input[type=radio][name=dateFormat]').change(function () {
        var val = $(this).val();
        if (val != "BS") {
            $('#BSCitizenshipIssueDate').hide();
            $('#CitizenshipIssueDate').show();
            if ($('#BSCitizenshipIssueDate').val() != '') {
                $('#CitizenshipIssueDate').val(GetShowFormattedDate(BS2AD($('#BSCitizenshipIssueDate').val())));

            }
        }
        else {

            $('#BSCitizenshipIssueDate').show();
            $('#CitizenshipIssueDate').hide();

            if ($('#CitizenshipIssueDate').val() != '') {
                $('#BSCitizenshipIssueDate').val(AD2BS(GetFormattedDate($('#CitizenshipIssueDate').val())));

            }

        }
    });
    $('input[type=radio][name=LicensedateFormat]').change(function () {
        var val = $(this).val();
        if (val != "BS") {
            $('#BSLicenseIssueDate, #BSLicenseExpireDate').hide();
            $('#LicenseIssueDate, #LicenseExpireDate').show();
            if ($('#BSLicenseIssueDate').val() != '' && $('#BSLicenseExpireDate').val() != '') {
                $('#LicenseIssueDate').val(GetShowFormattedDate(BS2AD($('#BSLicenseIssueDate').val())));
                $('#LicenseExpireDate').val(GetShowFormattedDate(BS2AD($('#BSLicenseExpireDate').val())));
            }
        }
        else {
            $('#BSLicenseIssueDate, #BSLicenseExpireDate').show();
            $('#LicenseIssueDate, #LicenseExpireDate').hide();
            if ($('#LicenseIssueDate').val() != '' && $('#LicenseExpireDate').val() != '') {
                $('#BSLicenseIssueDate').val(AD2BS(GetFormattedDate($('#LicenseIssueDate').val())));
                $('#BSLicenseExpireDate').val(AD2BS(GetFormattedDate($('#LicenseExpireDate').val())));
            }
        }
    });
});

function GetShowFormattedDate(date) {

    var date = new Date(date);
    var day = date.getDate();
    var month = (date.getMonth() + 1);
    if (month < 10) {
        month = '0' + month;
    }
    if (day < 10) {
        day = '0' + day;
    }
    return day + '/' + month + '/' + date.getFullYear()
    // return date.getDate() + ' ' + (date.getMonth() + 1) + ' ' + date.getFullYear()

}
function GetFormattedDate(date) {
    debugger;
    var initial = date.split(/\//);
    date = ([initial[1], initial[0], initial[2]].join('/')); //=> 'mm/dd/yyyy'

    var date = new Date(date);
    return date.getFullYear() + '-' + (date.getMonth() + 1) + '-' + date.getDate()

}

